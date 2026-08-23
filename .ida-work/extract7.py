from pathlib import Path

OUT = Path("/Users/bemly/cchaha/CharonAnchor/.ida-work/out")
OUT.mkdir(parents=True, exist_ok=True)

SITES = {
    "decode_rsphead": 0x63F9AAA,
    "decode_busibuff": 0x63F9BB0,
    "encrypt_body": 0x63FD660,
}

for label, ea in SITES.items():
    f = db.functions.get_at(ea)
    if not f:
        print(f"{label}: no func")
        continue
    start = f.start_ea
    size = f.end_ea - start
    print(f"{label}: func 0x{start:X} size={size}")
    if size > 120000:
        continue
    try:
        p = db.pseudocode.decompile(start)
        lines = p.to_text(remove_tags=True)
        (OUT / f"fn_{label}_0x{start:X}.c").write_text("\n".join(lines))
        print(f"  -> {len(lines)} lines")
    except Exception as e:
        print(f"  FAIL {e}")

# writer primitives
for label, ea in {"wr_u32": 0x8900B90, "wr_u8": 0x8900AA0, "wr_bytes": 0x63FAF50}.items():
    f = db.functions.get_at(ea)
    if not f: 
        print(f"{label}: no func"); continue
    try:
        p = db.pseudocode.decompile(f.start_ea)
        lines = p.to_text(remove_tags=True)
        (OUT / f"fn_{label}_0x{f.start_ea:X}.c").write_text("\n".join(lines))
        print(f"{label}: 0x{f.start_ea:X} {len(lines)} lines")
    except Exception as e:
        print(f"{label} FAIL {e}")

print("DONE7")
