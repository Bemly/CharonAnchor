from pathlib import Path

OUT = Path("/Users/bemly/cchaha/CharonAnchor/.ida-work/out")
OUT.mkdir(parents=True, exist_ok=True)

SITES = {
    "frame_decoder": 0x63EFD80,
    "frame_encoder": 0x63F8F44,
    "channel_tcp": 0x63E0EF0,
    "ecdh_v2_caller": 0x3411D04,
}

for label, ea in SITES.items():
    f = db.functions.get_at(ea)
    if not f:
        print(f"{label}: no func at 0x{ea:X}")
        continue
    start = f.start_ea
    size = f.end_ea - start
    name = db.functions.get_name(f)
    print(f"{label}: func 0x{start:X} size={size}")
    if size > 120000:
        print("  too big, skip decompile")
        continue
    try:
        p = db.pseudocode.decompile(start)
        lines = p.to_text(remove_tags=True)
        path = OUT / f"fn_{label}_0x{start:X}.c"
        path.write_text("\n".join(lines))
        print(f"  -> {len(lines)} lines")
    except Exception as e:
        print(f"  FAIL {type(e).__name__}: {e}")

print("DONE5")
