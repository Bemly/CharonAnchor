from pathlib import Path

OUT = Path("/Users/bemly/cchaha/CharonAnchor/.ida-work/out")
OUT.mkdir(parents=True, exist_ok=True)

FUNCS = {
    "derive16": 0x88DAB50,     # (share, 16, out) called in ECDH V2
    "enc_basic": 0x63F9050,
    "enc_busibuff": 0x63F9290,
    "enc_final": 0x63F9360,
    "parse_srv_pub": 0x8928AC0,
}

for label, ea in FUNCS.items():
    f = db.functions.get_at(ea)
    if not f:
        print(f"{label}: no func at 0x{ea:X}")
        continue
    start = f.start_ea
    size = f.end_ea - start
    print(f"{label}: func 0x{start:X} size={size}")
    if size > 120000:
        print("  too big")
        continue
    try:
        p = db.pseudocode.decompile(start)
        lines = p.to_text(remove_tags=True)
        path = OUT / f"fn_{label}_0x{start:X}.c"
        path.write_text("\n".join(lines))
        print(f"  -> {len(lines)} lines")
    except Exception as e:
        print(f"  FAIL {type(e).__name__}: {e}")

print("DONE6")
