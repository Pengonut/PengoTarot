#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;



namespace PengoTarot.Utils
{
    public static class MultiHitDetector
    {
        private static readonly ConcurrentDictionary<Type, bool> _cache = new();
        private static readonly Dictionary<short, OpCode> _opCodes;

        static MultiHitDetector()
        {
            _opCodes = new Dictionary<short, OpCode>();
            foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(OpCode))
                {
                    var op = (OpCode)field.GetValue(null)!;
                    _opCodes[op.Value] = op;
                }
            }
        }

        public static bool IsMultiHitCard(CardModel card)
        {
            Type type = card.GetType();
            if (_cache.TryGetValue(type, out bool cached))
                return cached;

            try
            {
                bool scanResult = ScanType(type);
                _cache[type] = scanResult;
                return scanResult;
            }
            catch (Exception ex)
            {
                Log.Error($"[PengoTarot.MultiHitDetector] Failed to scan {type.FullName}; assuming not multi-hit. {ex.GetType().Name}: {ex.Message}");
                _cache[type] = false;
                return false;
            }
        }

        private static bool ScanType(Type cardType)
        {
            var onPlay = cardType.GetMethod("OnPlay",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new[] { typeof(PlayerChoiceContext), typeof(CardPlay) },
                null);

            if (onPlay == null || onPlay.DeclaringType == typeof(CardModel))
                return false;

            MethodInfo? logic = GetActualLogicMethod(onPlay);
            if (logic == null)
                return false;

            var visited = new HashSet<MethodInfo>();
            return ScanMethod(logic, cardType, visited);
        }

        private static MethodInfo? GetActualLogicMethod(MethodInfo method)
        {
            var asyncAttr = method.GetCustomAttribute<AsyncStateMachineAttribute>();
            if (asyncAttr == null)
                return method;

            Type smType = asyncAttr.StateMachineType;
            var moveNext = smType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
            if (moveNext != null)
                return moveNext;

            foreach (var m in smType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (m.Name.Contains("MoveNext") && m.GetParameters().Length == 0)
                    return m;
            }
            return null;
        }

        private static bool ScanMethod(MethodInfo method, Type owningType, HashSet<MethodInfo> visited)
        {
            if (!visited.Add(method))
                return false;

            if (method.DeclaringType != owningType &&
                !owningType.IsAssignableFrom(method.DeclaringType) &&
                method.DeclaringType?.ReflectedType != owningType)
                return false;

            var body = method.GetMethodBody();
            if (body == null)
                return false;

            byte[]? il = body.GetILAsByteArray();
            if (il == null || il.Length == 0)
                return false;

            int maxIterations = il.Length * 2;
            int iterations = 0;
            int pos = 0;
            while (pos < il.Length)
            {
                short opVal = il[pos];
                int opSize = 1;
                if (opVal == 0xfe)
                {
                    if (pos + 1 >= il.Length) break;
                    opVal = (short)(0xfe00 | il[pos + 1]);
                    opSize = 2;
                }

                if (!_opCodes.TryGetValue(opVal, out var op))
                {
                    pos += opSize;
                    continue;
                }

                if (iterations++ > maxIterations)
                    return false;

                // Switch has a variable-length operand: uint32 count + count * int32 offsets
                if (op == OpCodes.Switch)
                {
                    int operandOff = pos + opSize;
                    if (operandOff + 4 <= il.Length)
                    {
                        uint count = BitConverter.ToUInt32(il, operandOff);
                        pos = operandOff + 4 + (int)count * 4;
                    }
                    else
                    {
                        pos += opSize;
                    }
                    continue;
                }

                int totalSize = opSize + GetOperandSize(op);
                if (totalSize < opSize)
                {
                    pos += opSize;
                    continue;
                }

                if (op == OpCodes.Call || op == OpCodes.Callvirt)
                {
                    int operandOff = pos + opSize;
                    if (operandOff + 4 <= il.Length)
                    {
                        int token = BitConverter.ToInt32(il, operandOff);
                        MethodInfo? called = null;
                        try
                        {
                            called = method.Module.ResolveMethod(token) as MethodInfo;
                        }
                        catch
                        {
                            // Token may be invalid if IL parsing got misaligned; skip safely.
                        }

                        if (called != null &&
                            called.Name == "WithHitCount" &&
                            called.GetParameters().Length == 1 &&
                            called.GetParameters()[0].ParameterType == typeof(int))
                        {
                            return true;
                        }

                        if (called?.DeclaringType != null &&
                            (called.DeclaringType == owningType || called.DeclaringType.DeclaringType == owningType))
                        {
                            MethodInfo? innerLogic = GetActualLogicMethod(called);
                            if (innerLogic != null && ScanMethod(innerLogic, owningType, visited))
                                return true;
                        }
                    }
                }

                pos += totalSize;
            }

            return false;
        }

        private static int GetOperandSize(OpCode op)
        {
            return op.OperandType switch
            {
                OperandType.InlineBrTarget => 4,
                OperandType.InlineField => 4,
                OperandType.InlineI => 4,
                OperandType.InlineMethod => 4,
                OperandType.InlineSig => 4,
                OperandType.InlineString => 4,
                OperandType.InlineTok => 4,
                OperandType.InlineType => 4,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 => 8,
                OperandType.InlineR => 8,
                OperandType.ShortInlineBrTarget => 1,
                OperandType.ShortInlineI => 1,
                OperandType.ShortInlineR => 4,
                OperandType.ShortInlineVar => 1,
                _ => 0
            };
        }
    }
}