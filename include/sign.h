#ifndef SIGN_H
#define SIGN_H

#include <stdint.h>

// 签名函数类型
// func(cmd, src, src_len, seq, out_buf)
typedef long (*sign_func_t)(const char* cmd, const uint8_t* src, int src_len, int seq, uint8_t* out);

// 初始化加载 wrapper.node
// libs: 预加载的依赖库列表
// libs_count: 库数量
// offset: 签名函数偏移地址
int sign_init(const char** libs, int libs_count, uintptr_t offset);

// 执行签名
int sign_call(const char* cmd, const uint8_t* src, int src_len, int seq, uint8_t* out);

// 从输出缓冲区提取 token/extra/sign
// buf: 输出缓冲区 (0x300 bytes)
void sign_extract(const uint8_t* buf,
                  uint8_t* token, int* token_len,
                  uint8_t* extra, int* extra_len,
                  uint8_t* sign, int* sign_len);

// 清理
void sign_cleanup();

#endif