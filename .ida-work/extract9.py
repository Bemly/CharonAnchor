from pathlib import Path

OUT = Path("/Users/bemly/cchaha/CharonAnchor/.ida-work/out")
OUT.mkdir(parents=True, exist_ok=True)

FUNCS = {
    "rd_init": 0x8901640,
    "rd_u32": 0x8901700,
    "rd_skip": 0x8901650,
    "rd_back": 0x8901690,
    "rd_u32b": 0x89016B0,
    "rd_str_a": 0x63FAC30,
    "rd_str_b": 0x63FACE0,
    "rd_vlint": 0x603CEB0,
}

for label, ea in FUNCS.items():
    f = db.functions.get_at(ea)
    if not f:
        print(f"{label}: no func at 0x{ea:X}")
        continue
    start = f.start_ea
    print(f"{label}: func 0x{start:X} size={f.end_ea-start}")
    try:
        p = db.pseudocode.decompile(start)
        lines = p.to_text(remove_tags=True)
        (OUT / f"fn_{label}_0x{start:X}.c").write_text("\n".join(lines))
        print(f"  -> {len(lines)} lines")
    except Exception as e:
        print(f"  FAIL {e}")

print("DONE9")
