from pathlib import Path

OUT = Path("/Users/bemly/cchaha/CharonAnchor/.ida-work/out")
OUT.mkdir(parents=True, exist_ok=True)

# EA of an instruction inside each target function (from objdump analysis)
SITES = {
    "ecdh_v2_gen": 0x3412793,        # ECDH Key V2 generation flow
    "ecdh_v2_cipherver": 0x3413219,  # cipher_ver/key_ver log nearby
    "aeskey_set": 0x6586b09,         # setting new aes key
    "rekey_state": 0x659407f,        # reKey to no/a/with new aes key
    "conn_seq": 0x64e0cc7,           # client_conn_seq
    "connector": 0x63ddfb1,          # tcp_channel_connector.cc log site
}

targets = set()
for label, ea in SITES.items():
    try:
        f = db.functions.get_at(ea)
    except Exception as e:
        print(f"{label}: lookup error {e}")
        continue
    if f:
        start = f.start_ea
        size = f.end_ea - f.start_ea
        print(f"{label}: func 0x{start:X}-0x{f.end_ea:X} size={size}")
        targets.add((label, start, f.end_ea))
    else:
        print(f"{label}: NO FUNCTION at 0x{ea:X}")

for label, start, end in sorted(targets):
    f = db.functions.get_at(start)
    name = db.functions.get_name(f)
    safe = str(name).replace("/", "_")[:60]
    p = OUT / f"fn_{label}_0x{start:X}.c"
    if end - start > 200000:
        print(f"skip {label} (too big)")
        continue
    try:
        lines = db.functions.get_pseudocode(f)
        p.write_text("\n".join(lines))
        print(f"wrote {p.name} ({len(lines)} lines)")
    except Exception as e:
        print(f"decomp fail {label}: {e}")

print("DONE3")
