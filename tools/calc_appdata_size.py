"""
计算 C:/Users/Pengo/AppData 下各二级目录（子目录的子目录）所占磁盘空间。
"""

import os
from pathlib import Path

APP_DATA = Path(r"C:\Users\Pengo\AppData")


def get_dir_size(path: Path) -> int:
    """递归计算目录总大小（字节）"""
    total = 0
    try:
        for entry in os.scandir(path):
            try:
                if entry.is_file(follow_symlinks=False):
                    total += entry.stat().st_size
                elif entry.is_dir(follow_symlinks=False):
                    total += get_dir_size(entry.path)
            except (PermissionError, OSError):
                continue
    except (PermissionError, OSError):
        pass
    return total


def fmt_size(size_bytes: int) -> str:
    """将字节数转为可读格式"""
    for unit in ("B", "KB", "MB", "GB", "TB"):
        if size_bytes < 1024:
            return f"{size_bytes:.2f} {unit}"
        size_bytes /= 1024
    return f"{size_bytes:.2f} PB"


def main():
    if not APP_DATA.exists():
        print(f"路径不存在: {APP_DATA}")
        return

    output = []
    output.append(f"{'二级目录':<50} {'大小':>10}")
    output.append("-" * 62)

    results: list[tuple[str, int]] = []

    for child in sorted(APP_DATA.iterdir()):
        if not child.is_dir():
            continue
        for grandchild in sorted(child.iterdir()):
            if not grandchild.is_dir():
                continue
            rel = grandchild.relative_to(APP_DATA).as_posix()
            sz = get_dir_size(grandchild)
            results.append((rel, sz))

    results.sort(key=lambda x: x[1], reverse=True)

    for rel, sz in results:
        output.append(f"{rel:<50} {fmt_size(sz):>10}")

    total_all = sum(sz for _, sz in results)
    output.append("-" * 62)
    output.append(f"{'总计':<50} {fmt_size(total_all):>10}")

    out_path = Path(__file__).parent / "appdata_size_result.txt"
    out_path.write_text("\n".join(output), encoding="utf-8")
    print(f"结果已写入: {out_path}")
    # 同时打印到控制台
    for line in output:
        print(line)


if __name__ == "__main__":
    main()
