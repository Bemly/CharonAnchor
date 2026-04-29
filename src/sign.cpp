#include "sign.h"
#include <dlfcn.h>
#include <link.h>
#include <cstring>
#include <cstdio>

static void* g_module = nullptr;
static uintptr_t g_base = 0;
static sign_func_t g_sign_func = nullptr;

// 输出缓冲区布局 (与原项目一致)
#define TOKEN_DATA_OFFSET 0x000
#define TOKEN_LEN_OFFSET  0x0FF
#define EXTRA_DATA_OFFSET 0x100
#define EXTRA_LEN_OFFSET  0x1FF
#define SIGN_DATA_OFFSET  0x200
#define SIGN_LEN_OFFSET   0x2FF

// dl_iterate_phdr 回调，查找 wrapper.node 基地址
static int find_module(struct dl_phdr_info* info, size_t size, void* data) {
    if (info->dlpi_name && strstr(info->dlpi_name, "wrapper.node")) {
        g_base = info->dlpi_addr;
        printf("Found wrapper.node at base: 0x%lx\n", g_base);
        return 1;
    }
    return 0;
}

int sign_init(const char** libs, int libs_count, uintptr_t offset) {
    // 1. 预加载依赖库
    for (int i = 0; i < libs_count; i++) {
        void* handle = dlopen(libs[i], RTLD_LAZY | RTLD_GLOBAL);
        if (handle) {
            printf("Preloaded: %s\n", libs[i]);
        } else {
            printf("Failed to preload %s: %s\n", libs[i], dlerror());
        }
    }

    // 2. 加载 wrapper.node
    g_module = dlopen("./wrapper.node", RTLD_LAZY);
    if (!g_module) {
        fprintf(stderr, "dlopen wrapper.node failed: %s\n", dlerror());
        return -1;
    }

    // 3. 查找基地址
    g_base = 0;
    dl_iterate_phdr(find_module, nullptr);

    if (g_base == 0) {
        fprintf(stderr, "Failed to find wrapper.node base address\n");
        dlclose(g_module);
        g_module = nullptr;
        return -1;
    }

    // 4. 计算签名函数地址
    g_sign_func = (sign_func_t)(g_base + offset);
    printf("Sign function at: 0x%lx\n", (uintptr_t)g_sign_func);

    // 验证地址有效性
    if ((uintptr_t)g_sign_func < 0x10000) {
        fprintf(stderr, "Invalid function address\n");
        dlclose(g_module);
        g_module = nullptr;
        return -1;
    }

    return 0;
}

int sign_call(const char* cmd, const uint8_t* src, int src_len, int seq, uint8_t* out) {
    if (!g_sign_func) {
        return -1;
    }

    // 调用签名函数
    long ret = g_sign_func(cmd, src, src_len, seq, out);

    return (ret == 0) ? 0 : -1;
}

void sign_extract(const uint8_t* buf,
                  uint8_t* token, int* token_len,
                  uint8_t* extra, int* extra_len,
                  uint8_t* sign, int* sign_len) {
    // Token
    *token_len = buf[TOKEN_LEN_OFFSET];
    memcpy(token, buf + TOKEN_DATA_OFFSET, *token_len);

    // Extra
    *extra_len = buf[EXTRA_LEN_OFFSET];
    memcpy(extra, buf + EXTRA_DATA_OFFSET, *extra_len);

    // Sign
    *sign_len = buf[SIGN_LEN_OFFSET];
    memcpy(sign, buf + SIGN_DATA_OFFSET, *sign_len);
}

void sign_cleanup() {
    if (g_module) {
        dlclose(g_module);
        g_module = nullptr;
        g_sign_func = nullptr;
    }
}