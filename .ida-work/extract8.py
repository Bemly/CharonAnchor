from pathlib import Path

OUT = Path("/Users/bemly/cchaha/CharonAnchor/.ida-work/out")
OUT.mkdir(parents=True, exist_ok=True)

FUNCS = {
    "aes_padsize": 0x85BC510,
    "aes_crypt": 0x85BC530,
    "decode_basic": 0x63F9D40,
    "decode_rsphead2": 0x63F9E40,
    "decode_busibuff2": 0x63F9FF0,
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

# callers of EncodePacket 0x63F8BA0 and DecryptBody 0x63FD870
for label, ea in [("EncodePacket", 0x63F8BA0), ("DecryptBody", 0x63FD870)]:
    f = db.functions.get_at(ea)
    try:
        cl = sorted({c.ea for c in db.xrefs.get_callers(f.start_ea)})
        print(f"callers of {label}: {[hex(x) for x in cl]}")
    except Exception as e:
        print(f"{label} callers FAIL: {e}")

print("DONE8")
