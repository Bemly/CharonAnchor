import json
from pathlib import Path

OUT = Path("/Users/bemly/cchaha/CharonAnchor/.ida-work/out")
OUT.mkdir(parents=True, exist_ok=True)

ANCHORS = {}
# find anchor strings
targets = [
    "Generate ECDH Key V2 Succeed",
    "reKey to no aes key",
    "msf_session_impl.cc",
    "tcp_channel_connector.cc",
    "ECDH pub key info: cipher_ver",
    "ComputeShareKey failed",
    "GenerateAesKey",
]
addr_map = {}
for s in db.strings:
    try:
        text = str(s)
    except Exception:
        continue
    for t in targets:
        if t in text and t not in addr_map:
            addr_map[t] = s.address

for t, a in addr_map.items():
    print(f"STRING 0x{a:X}: {t}")

# functions containing xrefs to anchors
def func_at(ea):
    try:
        return db.functions.get_function_by_ea(ea)
    except Exception:
        return None

report = {"strings": {t: f"0x{a:X}" for t, a in addr_map.items()}, "funcs": []}

for t, a in addr_map.items():
    callers = set()
    for xref in db.xrefs.to_ea(a):
        f = func_at(xref.from_ea)
        if f:
            callers.add(f.start_ea)
    for ca in sorted(callers):
        report["funcs"].append({"anchor": t, "func": f"0x{ca:X}"})
        print(f"XREF: {t} <- func 0x{ca:X}")

# decompile each unique function + write pseudocode files
uniq = sorted({int(r["func"], 16) for r in report["funcs"]})
for ea in uniq:
    f = func_at(ea)
    if not f:
        continue
    name = db.functions.get_name(f)
    print(f"DECOMP 0x{ea:X} {name} size={f.end_ea - f.start_ea}")
    try:
        lines = db.functions.get_pseudocode(f)
        safe = name.replace("/", "_")
        p = OUT / f"decomp_0x{ea:X}_{safe}.c"
        p.write_text("\n".join(lines))
        print(f"  -> {p}")
        report["funcs"] = [r for r in report["funcs"]]
    except Exception as e:
        print(f"  decomp failed: {e}")

# also decompile direct callees/callers one hop from ECDH V2 generator
(OUT / "report.json").write_text(json.dumps(report, indent=2))
print("DONE")
