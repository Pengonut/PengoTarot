// PengoTarot: Rewrite all third-party GetNode calls to our redirect,
// but only perform recursive FindChild when the instance is a card Body.
// Logs each distinct call site once per session for transparency.

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace PengoTarot.BalatroEffect
{
    internal static class GetNodeCallRewriter
    {
        private static readonly MethodInfo GenericGetNodeDef;
        private static readonly MethodInfo NonGenericGetNode;
        private static readonly MethodInfo RedirectGenericMethod;
        private static readonly MethodInfo RedirectNonGenericMethod;
        private static readonly MethodInfo GenericGetNodeOrNullDef;
        private static readonly MethodInfo NonGenericGetNodeOrNull;
        private static readonly MethodInfo RedirectGenericOrNullMethod;
        private static readonly MethodInfo RedirectNonGenericOrNullMethod;

        private static Harmony? _harmony;
        private static bool _hasRun;
        private static int _deferAttempts;

        // Track which (callerDescription, path) pairs we have already logged.
        private static readonly ConcurrentDictionary<string, byte> LoggedSites = new();

        static GetNodeCallRewriter()
        {
            GenericGetNodeDef = typeof(Node).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(m => m.IsGenericMethod && m.Name == "GetNode" &&
                            m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(NodePath));

            NonGenericGetNode = typeof(Node).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(m => !m.IsGenericMethod && m.Name == "GetNode" &&
                            m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(NodePath));

            RedirectGenericMethod = typeof(GetNodeCallRewriter).GetMethod(nameof(RedirectGetNodeGeneric),
                BindingFlags.NonPublic | BindingFlags.Static)!;
            RedirectNonGenericMethod = typeof(GetNodeCallRewriter).GetMethod(nameof(RedirectGetNode),
                BindingFlags.NonPublic | BindingFlags.Static)!;

            // GetNodeOrNull（泛型 + 非泛型）：Godot 4.5.1 Node 均存在（GodotSharp.xml 已核实）
            GenericGetNodeOrNullDef = typeof(Node).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(m => m.IsGenericMethod && m.Name == "GetNodeOrNull" &&
                            m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(NodePath));

            NonGenericGetNodeOrNull = typeof(Node).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(m => !m.IsGenericMethod && m.Name == "GetNodeOrNull" &&
                            m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(NodePath));

            RedirectGenericOrNullMethod = typeof(GetNodeCallRewriter).GetMethod(nameof(RedirectGetNodeOrNullGeneric),
                BindingFlags.NonPublic | BindingFlags.Static)!;
            RedirectNonGenericOrNullMethod = typeof(GetNodeCallRewriter).GetMethod(nameof(RedirectGetNodeOrNull),
                BindingFlags.NonPublic | BindingFlags.Static)!;
        }

        public static void ScheduleAfterAllModsLoaded()
        {
            try
            {
                Callable.From((Action)RunDeferred).CallDeferred();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PengoTarot] Could not schedule GetNode rewrite pass: {ex.Message}");
            }
        }

        private static void RunDeferred()
        {
            if (_hasRun) return;
            if (ModManager.State != ModManagerState.Initialized)
            {
                if (++_deferAttempts < 300)
                {
                    Callable.From((Action)RunDeferred).CallDeferred();
                }
                else
                {
                    GD.PrintErr("[PengoTarot] ModManager never Initialized; GetNode rewrite skipped.");
                }
                return;
            }

            _hasRun = true;
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PengoTarot] GetNode rewrite pass failed: {ex}");
            }
        }

        private static void Run()
        {
            _harmony = new Harmony("com.pengotarot.getnoderewrite");
            Assembly self = typeof(GetNodeCallRewriter).Assembly;

            foreach (Mod mod in ModManager.Mods)
            {
                if (mod.state != ModLoadState.Loaded) continue;

                IEnumerable<Assembly> assembliesToScan;
#if STS2_AT_LEAST_0_110_0
                assembliesToScan = mod.assemblies ?? Enumerable.Empty<Assembly>();
#else
                var single = mod.assembly;
                assembliesToScan = single != null ? new[] { single } : Enumerable.Empty<Assembly>();
#endif

                foreach (Assembly modAssembly in assembliesToScan)
                {
                    if (modAssembly == null || modAssembly == self) continue;
                    ScanAssembly(modAssembly);
                }
            }
        }

        private static void ScanAssembly(Assembly assembly)
        {
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }
            catch { return; }

            foreach (Type type in types)
            {
                foreach (MethodBase method in EnumerateMethods(type))
                {
                    ScanMethod(method);
                }
            }
        }

        private static IEnumerable<MethodBase> EnumerateMethods(Type type)
        {
            const BindingFlags All = BindingFlags.Instance | BindingFlags.Static |
                                     BindingFlags.Public | BindingFlags.NonPublic |
                                     BindingFlags.DeclaredOnly;
            foreach (ConstructorInfo ctor in type.GetConstructors(All)) yield return ctor;
            foreach (MethodInfo method in type.GetMethods(All)) yield return method;
        }

        private static void ScanMethod(MethodBase method)
        {
            if (method.IsAbstract || method.ContainsGenericParameters || (method.DeclaringType?.ContainsGenericParameters ?? false))
                return;

            byte[]? il;
            try { il = method.GetMethodBody()?.GetILAsByteArray(); }
            catch { return; }
            if (il == null) return;

            bool hasCall = false;
            foreach (byte b in il)
                if (b == 0x6F || b == 0x28) { hasCall = true; break; }
            if (!hasCall) return;

            List<KeyValuePair<OpCode, object>>? codes;
            try { codes = PatchProcessor.ReadMethodBody(method).ToList(); }
            catch { return; }

            bool hasGetNode = false;
            foreach (var pair in codes)
            {
                if ((pair.Key == OpCodes.Callvirt || pair.Key == OpCodes.Call) &&
                    pair.Value is MethodInfo called &&
                    IsGetNodeLike(called))
                {
                    hasGetNode = true;
                    break;
                }
            }
            if (!hasGetNode) return;

            try
            {
                _harmony!.Patch(method, transpiler: new HarmonyMethod(typeof(GetNodeCallRewriter), nameof(Transpiler)));
            }
            catch { }
        }

        /// <summary>是否 GetNode / GetNodeOrNull（泛型或非泛型）之一。</summary>
        private static bool IsGetNodeLike(MethodInfo method)
        {
            if (!method.IsGenericMethod)
                return method.Equals(NonGenericGetNode) || method.Equals(NonGenericGetNodeOrNull);
            var def = method.GetGenericMethodDefinition();
            return def.Equals(GenericGetNodeDef) || def.Equals(GenericGetNodeOrNullDef);
        }

        // Transpiler: replace every GetNode/GetNodeOrNull call with our static redirect
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction instr = codes[i];
                if (instr.opcode != OpCodes.Callvirt && instr.opcode != OpCodes.Call) continue;
                if (instr.operand is not MethodInfo method) continue;

                MethodInfo? targetMethod = ResolveRedirectTarget(method);
                if (targetMethod == null) continue;

                var newInstr = new CodeInstruction(OpCodes.Call, targetMethod);
                newInstr.labels.AddRange(instr.labels);
                newInstr.blocks.AddRange(instr.blocks);
                codes[i] = newInstr;
            }
            return codes;
        }

        // 根据被调用方法选择重定向目标（GetNode / GetNodeOrNull 各自泛型与非泛型）
        private static MethodInfo? ResolveRedirectTarget(MethodInfo method)
        {
            if (method.IsGenericMethod)
            {
                var def = method.GetGenericMethodDefinition();
                if (def.Equals(GenericGetNodeDef))
                    return RedirectGenericMethod.MakeGenericMethod(method.GetGenericArguments());
                if (def.Equals(GenericGetNodeOrNullDef))
                    return RedirectGenericOrNullMethod.MakeGenericMethod(method.GetGenericArguments());
                return null;
            }
            if (method.Equals(NonGenericGetNode))
                return RedirectNonGenericMethod;
            if (method.Equals(NonGenericGetNodeOrNull))
                return RedirectNonGenericOrNullMethod;
            return null;
        }

        // ── Redirects ─────────────────────────────────────────────────────

        private static Node RedirectGetNode(Node instance, NodePath path)
        {
            return RedirectGetNodeInternal<object>(instance, path, generic: false) ?? instance.GetNode(path);
        }

        private static T RedirectGetNodeGeneric<T>(Node instance, NodePath path) where T : class
        {
            Node? result = RedirectGetNodeInternal<T>(instance, path, generic: true);
            if (result is T typed) return typed;
            return instance.GetNode<T>(path);
        }

        // ── GetNodeOrNull redirects（崩坠 CardOverlayPatches.Sync 等依赖） ────────────

        private static Node? RedirectGetNodeOrNull(Node instance, NodePath path)
        {
            Node? result = RedirectGetNodeOrNullInternal(instance, path);
            return result ?? instance.GetNodeOrNull(path);
        }

        private static T? RedirectGetNodeOrNullGeneric<T>(Node instance, NodePath path) where T : class
        {
            Node? result = RedirectGetNodeOrNullInternal(instance, path);
            if (result is T typed) return typed;
            return instance.GetNodeOrNull<T>(path);
        }

        // GetNodeOrNull 语义：找不到返回 null。
        // 只在 NCard 相关 + 单段路径时做递归 FindChild；不触发性能节流（找不到是判空常态）。
        private static Node? RedirectGetNodeOrNullInternal(Node instance, NodePath path)
        {
            string pathStr = path.ToString();
            if (pathStr.Contains('/')) return null;
            if (!(instance is NCard || instance.GetParent() is NCard)) return null;

            string searchName = pathStr.StartsWith("%") ? pathStr.Substring(1) : pathStr;
            Node? found = instance.FindChild(searchName, recursive: true, owned: false);

            string caller = GetCallerDescription();
            string key = $"{caller}|or-null:{pathStr}";
            if (found != null && LoggedSites.TryAdd(key, 0))
            {
                string location = found.IsInsideTree() ? found.GetPath().ToString() : "(not in tree)";
                GD.Print($"[PengoTarot] GetNodeOrNull{{\"{pathStr}\"}} from [{caller}] -> found '{found.Name}' at {location}");
            }
            return found;
        }

        // Common logic: if instance is a card Body, try recursive FindChild.
        // Returns the found node or null if we should fall through to original.
        // Logs once per unique (callerDescription, path).
        private static Node? RedirectGetNodeInternal<T>(Node instance, NodePath path, bool generic)
        {
            // Only intercept calls on card Body
            if (!(instance is NCard || instance.GetParent() is NCard))
                return null;

            string pathStr = path.ToString();
            if (pathStr.Contains('/'))
                return null; // multi-segment paths are safe

            string searchName = pathStr;
            if (searchName.StartsWith("%"))
                searchName = searchName.Substring(1);

            Node? found = instance.FindChild(searchName, recursive: true, owned: false);

            // Generate a stable caller description
            string caller = GetCallerDescription();

            // Unique key for this call site
            string key = $"{caller}|{pathStr}";

            if (LoggedSites.TryAdd(key, 0))
            {
                if (found != null)
                {
                    string location = found.IsInsideTree() ? found.GetPath().ToString() : "(not in tree)";
                    GD.Print($"[PengoTarot] GetNode{{\"{pathStr}\"}} from [{caller}] -> found '{found.Name}' at {location}");
                }
                else
                {
                    GD.Print($"[PengoTarot] GetNode{{\"{pathStr}\"}} from [{caller}] -> NOT FOUND on card body. Triggering emergency throttle.");
                    if (!Config.IsPerformanceThrottled)
                        Config.SetPerformanceThrottled(true);
                }
            }

            return found;
        }

        // Produce a compact mod/method identifier from the stack trace.
        private static string GetCallerDescription()
        {
            // Skip our own frames: this method + redirect method + transpiled method
            var trace = new System.Diagnostics.StackTrace(3, false);
            for (int i = 0; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);
                var method = frame?.GetMethod();
                if (method == null) continue;
                if (method.DeclaringType == typeof(GetNodeCallRewriter)) continue;
                string assembly = method.DeclaringType?.Assembly.GetName().Name ?? "?";
                string type = method.DeclaringType?.FullName ?? "?";
                return $"{assembly} | {type}.{method.Name}";
            }
            return "unknown";
        }
    }
}