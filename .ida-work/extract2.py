import json
from pathlib import Path

OUT = Path("/Users/bemly/cchaha/CharonAnchor/.ida-work/out")
OUT.mkdir(parents=True, exist_ok=True)

# Frame-format anchors
TARGETS = {
    "Heartbeat.Alive": 0xa1f424,
    "client_conn_seq": 0x999579,
    "tcp_channel_connector.cc": 0x9140a7,
    "codec_processor_v12.cc": 0xa3ae43,
    "codec_processor_v13.cc": 0x8c1f42,
    "codec_processor_v20.cc": 0x8c1f94,
    "codec_processor_v21.cc": 0x9e988e,
    "reKey to no aes key": 0x83f22f,
    "setting new aes key": 0xaa49aa,
    "reKey to a aes key": 0x9b470c,
    "reKey with new aes key": 0x9ce9d4,
}

def func_of(ea):
    try:
        return db.functions.get_function_by_ea(ea)
    except Exception:
        return None

report = {}
for name, addr in TARGETS.items():
    funcs = set()
    try:
        for xref in db.xrefs.to_ea(addr):
            f = func_of(xref.from_ea)
            if f:
                funcs.add(f.start_ea)
    except Exception as e:
        print(f"xref fail {name}: {e}")
    report[name] = sorted(funcs)
    print(f"{name} @0x{addr:X}: {len(funcs)} funcs -> {[hex(f) for f in sorted(funcs)]}")

# decompile unique functions (cap size to keep output sane)
uniq = sorted({ea for lst in report.values() for ea in lst})
print(f"\ntotal unique funcs: {len(uniq)}")
for ea in uniq[:40]:
    f = func_of(ea)
    if not f:
        continue
    name = db.functions.get_name(f)
    size = f.end_ea - f.start_ea
    print(f"DECOMP 0x{ea:X} {name} size={size}")
    if size > 60000:
        print("  skipped (too large)")
        continue
    try:
        lines = db.functions.get_pseudocode(f)
        safe = str(name).replace("/", "_").replace("<", "_")[:80]
        p = OUT / f"frame_0x{ea:X}_{safe}.c"
        p.write_text("\n".join(lines))
        print(f"  -> {p.name}")
    except Exception as e:
        print(f"  FAIL {e}")

(OUT / "frame_report.json").write_text(json.dumps({k: [hex(x) for x in v] for k, v in report.items()}, indent=2))
print("DONE2")
