#!/bin/bash

# CharonAnchor 编译脚本

set -e

# 编译 libsymbols.so
echo "Building libsymbols.so..."
gcc -shared -fPIC -o libsymbols.so symbols.c

# 编译 charon (主程序)
echo "Building charon..."
g++ -std=c++17 -O2 -o charon \
    src/main.cpp src/sign.cpp \
    -I include -I third_party \
    -ldl

echo "Build complete!"
echo ""
echo "Files:"
ls -lh charon libsymbols.so

echo ""
echo "Usage:"
echo "  1. Place wrapper.node, libsymbols.so, libbugly.so, libcrbase.so in this directory"
echo "  2. Run ./charon"