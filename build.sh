#!/bin/bash

# CharonAnchor 编译脚本
# 用法: ./build.sh [版本]
# 版本: 3.2.19-39038 (默认), 3.2.28-48517

set -e

VERSION=${1:-3.2.19-39038}

case $VERSION in
    3.2.19-39038)
        SIGN_OFFSET=0x5ADE220
        ;;
    3.2.28-48517)
        SIGN_OFFSET=0x56D5491
        ;;
    *)
        echo "未知版本: $VERSION"
        echo "支持版本: 3.2.19-39038, 3.2.28-48517"
        exit 1
        ;;
esac

echo "编译版本: $VERSION"
echo "签名偏移: 0x$(printf '%X' $SIGN_OFFSET)"

# 编译 libsymbols.so
echo "Building libsymbols.so..."
gcc -shared -fPIC -o libsymbols.so symbols.c

# 编译 charon (主程序)
echo "Building charon..."
g++ -std=c++17 -O2 -o charon \
    src/main.cpp src/sign.cpp \
    -I include -I third_party \
    -ldl \
    -DSIGN_OFFSET=$SIGN_OFFSET

echo "Build complete!"
echo ""
echo "Files:"
ls -lh charon libsymbols.so

echo ""
echo "Usage:"
echo "  1. Place wrapper.node, libsymbols.so, libbugly.so, libcrbase.so in this directory"
echo "  2. Run ./charon"