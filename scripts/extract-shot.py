#!/usr/bin/env python3
"""Extract the base64 PNG from a saved MCP wait-capture result file: extract-shot.py <result.txt> <out.png>"""
import base64, json, sys
d = json.load(open(sys.argv[1]))
b = d.get("capture", {}).get("base64")
if not b:
    print("no base64 in", list(d.get("capture", {}).keys())); sys.exit(1)
open(sys.argv[2], "wb").write(base64.b64decode(b))
print("ok", sys.argv[2])
