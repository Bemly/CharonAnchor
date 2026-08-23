from pathlib import Path

OUT = Path("/Users/bemly/cchaha/CharonAnchor/.ida-work/out")
OUT.mkdir(parents=True, exist_ok=True)

FUNCS = {
    "ecdh_v2_gen": 0x3412410,
    "ecdh_v2_cipherver": 0x3413140,
    "aeskey_set": 0x6586AB0,
    "rekey_state": 0x6593F00,
    "conn_seq": 0x64E05D0,
    "connector": 0x63DDEB0,
}

for label, ea in FUNCS.items():
    f = db.functions.get_at(ea)
    if not f:
        print(f"{label}: no func at 0x{ea:X}")
        continue
    name = db.functions.get_name(f)
    try:
        p = db.pseudocode.decompile(ea)
        lines = p.to_text(remove_tags=True)
        path = OUT / f"fn_{label}_0x{ea:X}.c"
        path.write_text("\n".join(lines))
        print(f"{label}: decompiled {len(lines)} lines -> {path.name}")
    except Exception as e:
        print(f"{label}: decomp FAIL {type(e).__name__}: {e}")
        # fallback disassembly
        d = db.functions.get_disassembly(f, remove_tags=True)
        path = OUT / f"fn_{label}_0x{ea:X}.asm"
        path.write_text("\n".join(d))
        print(f"   fallback asm {len(d)} lines")

# callers of rekey_state and ecdh gen
def callers_of(ea):
    f = db.functions.get_at(ea)
    out = []
    for c in db.xrefs.get_callers(f.start_ea):
        out.append(c.ea)
    return sorted(set(out))

for label, ea in [("rekey_state", 0x6593F00), ("ecdh_v2_gen", 0x3412410), ("aeskey_set", 0x6586AB0)]:
    try:
        cl = callers_of(ea)
        print(f"callers of {label}: {[hex(x) for x in cl]}")
    except Exception as e:
        print(f"callers of {label} FAIL: {e}")

print("DONE4")
