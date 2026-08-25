#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""PengoTarot Steam Workshop 上传脚本（uploader.bat 的核心逻辑）。

流程：
1. 同步内容：把 游戏 mods 目录 (D:\\[Game] Steam\\...\\mods\\PengoTarot)
   复制到 工坊内容目录 (D:\\[Tool] Godot\\PengoTarot\\content)。
   只覆盖同名 + 新增，不删除、不清理目标里多余文件。
2. 写元数据：从 workshop_meta.json 读 description -> workshop.json；
   从 CHANGELOG.json 当前版本块读 changes -> workshop.json 的 changeNote。
3. 上传：调用 ModUploader.exe upload -w PengoTarot。
4. 上传成功后：当前版本块写入 released 日期，自动创建版本号 +1 的新空块，
   并把 current_version 指向新块。失败则不建块，可直接重跑。

支持 --dry-run：只打印将执行的步骤，不修改任何文件、不上传。
"""

import argparse
import codecs
import json
import re
import shutil
import subprocess
import sys
from datetime import date
from pathlib import Path

# ===== 路径常量（硬编码，与 cwd 无关）=====
GAME_MODS_DIR = Path(r"D:\[Game] Steam\steamapps\common\Slay the Spire 2\mods\PengoTarot")
WORKSHOP_DIR = Path(r"D:\[Tool] Godot\PengoTarot")
CONTENT_DIR = WORKSHOP_DIR / "content"
MOD_UPLOADER = Path(r"D:\[Tool] Godot\ModUploader.exe")
CHANGELOG_PATH = Path(r"D:\PengoTarot\CHANGELOG.json")
WORKSHOP_META_PATH = Path(r"D:\PengoTarot\workshop_meta.json")
WORKSHOP_JSON_PATH = WORKSHOP_DIR / "workshop.json"

# 复制时忽略的顶层文件（README 无实际作用；历史 ZIP 是本地备份，不应进入工坊内容）
IGNORE_TOP_FILES = {"README.md"}


def _ignore_top_files(src: str, names):
    """copytree ignore 回调：忽略顶层 README 与本地历史 ZIP。"""
    if Path(src) == GAME_MODS_DIR:
        return {name for name in names if name in IGNORE_TOP_FILES or name.lower().endswith(".zip")}
    return set()


def sync_content(dry_run: bool) -> None:
    """功能 A：游戏 mods -> content，覆盖同名 + 新增，不删除。"""
    if not GAME_MODS_DIR.is_dir():
        sys.exit(f"[错误] 源目录不存在: {GAME_MODS_DIR}")
    print(f"[1/4] 同步内容: {GAME_MODS_DIR} -> {CONTENT_DIR}")
    if dry_run:
        print("      (dry-run) 将执行 copytree(dirs_exist_ok=True)，忽略顶层 README.md 与 *.zip")
        return
    shutil.copytree(
        GAME_MODS_DIR, CONTENT_DIR,
        dirs_exist_ok=True, ignore=_ignore_top_files,
    )
    print("      完成（覆盖同名 + 新增，未删除任何文件）")


def _read_preserve_bom(path: Path) -> tuple[str, bool]:
    """读取文本并检测是否带 UTF-8 BOM（workshop.json 由 Godot/编辑器生成可能带 BOM）。"""
    raw = path.read_bytes()
    has_bom = raw.startswith(codecs.BOM_UTF8)
    return raw.decode("utf-8-sig"), has_bom


def _write_preserve_bom(path: Path, text: str, has_bom: bool) -> None:
    """按原文件是否带 BOM 写回，保持文件编码风格不变。"""
    data = text.encode("utf-8")
    if has_bom:
        data = codecs.BOM_UTF8 + data
    path.write_bytes(data)


def _load_json(path: Path, label: str) -> dict:
    try:
        text, _ = _read_preserve_bom(path)
        return json.loads(text)
    except FileNotFoundError:
        sys.exit(f"[错误] 缺少 {label}: {path}")
    except json.JSONDecodeError as e:
        sys.exit(f"[错误] {label} 不是合法 JSON（{path}）: {e}")


def update_workshop_json(dry_run: bool) -> tuple[str, str]:
    """功能 B：写 workshop.json 的 description（来自 workshop_meta.json）与 changeNote（来自 CHANGELOG 当前块）。"""
    print("[2/4] 更新 workshop.json")
    meta = _load_json(WORKSHOP_META_PATH, "workshop_meta.json")
    changelog = _load_json(CHANGELOG_PATH, "CHANGELOG.json")
    ws_text, ws_bom = _read_preserve_bom(WORKSHOP_JSON_PATH)
    try:
        ws = json.loads(ws_text)
    except json.JSONDecodeError as e:
        sys.exit(f"[错误] workshop.json 不是合法 JSON: {e}")

    ver = changelog.get("current_version")
    desc = (meta.get("description") or "").strip()
    if desc:
        # 工坊长描述底部版本号始终跟随 CHANGELOG 当前发布块，避免手动维护遗漏。
        desc = re.sub(r"(?m)^Version:\s*[^\r\n]+$", f"Version: {ver.removeprefix('v')}", desc)
        ws["description"] = desc
    else:
        print("      [警告] workshop_meta.json 的 description 为空，保留 workshop.json 原值（请手动维护）")

    block = (changelog.get("versions") or {}).get(ver)
    if not block:
        sys.exit(f"[错误] CHANGELOG.json 缺少 current_version 对应的块: {ver!r}")
    changes = block.get("changes") or []
    note_lines = [ver] + [f"- {c}" for c in changes]
    ws["changeNote"] = "\n".join(note_lines)

    print(f"      changeNote:\n{ws['changeNote']}")
    if dry_run:
        print("      (dry-run) 不写入文件")
        return ver, changes

    _write_preserve_bom(
        WORKSHOP_JSON_PATH,
        json.dumps(ws, ensure_ascii=False, indent=2) + "\n",
        ws_bom,
    )
    print("      已写入 workshop.json")
    return ver, changes


def run_uploader(dry_run: bool) -> bool:
    """步骤 3：调用 ModUploader 上传。返回是否成功。"""
    print(f"[3/4] 调用 ModUploader 上传 ({MOD_UPLOADER})")
    if dry_run:
        print("      (dry-run) 不调用上传")
        return False
    try:
        proc = subprocess.run(
            [str(MOD_UPLOADER), "upload", "-w", WORKSHOP_DIR.name],
            cwd=str(WORKSHOP_DIR.parent),
            capture_output=True, text=True,
            encoding="utf-8", errors="replace",
        )
    except FileNotFoundError:
        sys.exit(f"[错误] 找不到 ModUploader.exe: {MOD_UPLOADER}")
    out = (proc.stdout or "") + (proc.stderr or "")
    print(out)
    ok = proc.returncode == 0 and "Successfully uploaded" in out
    if not ok:
        print(f"[错误] 上传失败（退出码 {proc.returncode}），未创建新版本块，可直接重跑")
    return ok


def bump_version(ver: str) -> str:
    """版本号最后一段数字 +1。如 v1.4.10 -> v1.4.11、v2.0 -> v2.1。大版本跳升由用户手动改。"""
    m = re.fullmatch(r"(\D*)(\d+(?:\.\d+)*)(\D*)", ver)
    if not m:
        sys.exit(f"[错误] 无法自动 +1 版本号: {ver!r}（请手动修改 CHANGELOG.json）")
    prefix, nums, suffix = m.groups()
    parts = [int(p) for p in nums.split(".")]
    parts[-1] += 1
    return f"{prefix}{'.'.join(map(str, parts))}{suffix}"


def mark_released_and_create_next(changelog: dict, ver: str, dry_run: bool) -> None:
    """步骤 4：当前块写 released 日期，创建 +1 新块，更新 current_version。"""
    print("[4/4] 上传成功，更新 CHANGELOG.json")
    new_ver = bump_version(ver)
    changelog["versions"][ver]["released"] = date.today().isoformat()
    changelog["versions"][new_ver] = {"released": None, "changes": []}
    changelog["current_version"] = new_ver
    print(f"      {ver} -> released {date.today().isoformat()}")
    print(f"      创建新块 {new_ver}，current_version -> {new_ver}")
    if dry_run:
        print("      (dry-run) 不写入文件")
        return
    _, changelog_bom = _read_preserve_bom(CHANGELOG_PATH)
    _write_preserve_bom(
        CHANGELOG_PATH,
        json.dumps(changelog, ensure_ascii=False, indent=2) + "\n",
        changelog_bom,
    )
    print("      已写入 CHANGELOG.json")


def main() -> None:
    parser = argparse.ArgumentParser(description="PengoTarot Steam Workshop 上传")
    parser.add_argument("--dry-run", action="store_true", help="只打印将执行的步骤，不修改文件、不上传")
    parser.add_argument("--no-upload", action="store_true", help="实际执行同步与写 workshop.json，但不调用上传、不建新版本块")
    args = parser.parse_args()
    dry_run = args.dry_run

    sync_content(dry_run)
    ver, _ = update_workshop_json(dry_run)
    if args.no_upload:
        print("\n(--no-upload) 已同步内容并更新 workshop.json，未调用上传，因此不创建新版本块。")
        sys.exit(0)
    ok = run_uploader(dry_run)
    if not ok:
        if dry_run:
            print("\n(dry-run) 未执行上传，因此不创建新版本块。验证结束。")
            sys.exit(0)
        sys.exit(1)

    changelog = _load_json(CHANGELOG_PATH, "CHANGELOG.json")
    mark_released_and_create_next(changelog, ver, dry_run)
    print("\n上传完成！")


if __name__ == "__main__":
    main()
