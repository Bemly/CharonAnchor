// symbols.c - 辅助符号文件
// 编译: gcc -shared -fPIC -o libsymbols.so symbols.c

// 此文件提供空符号，用于解决 wrapper.node 的符号依赖问题

void __attribute__((weak)) dummy_symbol() {}