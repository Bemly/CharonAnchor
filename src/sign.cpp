#include "sign.h"
#include <dlfcn.h>
#include <link.h>
#include <cstring>
#include <cstdio>
#include <cstdlib>
#include <unistd.h>
#include <sys/stat.h>
#include <cerrno>

static void* g_module = nullptr;
static uintptr_t g_base = 0;
static sign_func_t g_sign_func = nullptr;

// 新版本签名函数类型
// int sub_57E1131(module_id, data, data_len, seq, output)
typedef int (*sign_func_v2_t)(const char*, const void*, int, int, unsigned char*);

// 签名函数指针（兼容新旧版本）
static sign_func_v2_t g_sign_func_v2 = nullptr;

// 输出缓冲区布局 (与原项目一致)
#define TOKEN_DATA_OFFSET 0x000
#define TOKEN_LEN_OFFSET  0x0FF
#define EXTRA_DATA_OFFSET 0x100
#define EXTRA_LEN_OFFSET  0x1FF
#define SIGN_DATA_OFFSET  0x200
#define SIGN_LEN_OFFSET   0x2FF

// dl_iterate_phdr 回调，查找 wrapper.node 基地址
static int find_module(struct dl_phdr_info* info, size_t size, void* data) {
    const char* target = (const char*)data;
    if (info->dlpi_name && strstr(info->dlpi_name, target ? target : "wrapper.node")) {
        g_base = info->dlpi_addr;
        printf("Found wrapper.node at base: 0x%lx (path: %s)\n", g_base, info->dlpi_name);
        return 1;
    }
    return 0;
}

// 创建包含 "DFLJ" 路径的符号链接来绕过反逆向检查
static char* create_bypass_symlink(const char* original_path) {
    static char symlink_path[256];

    // 创建临时目录 /tmp/DFLJ
    const char* tmp_dir = "/tmp/DFLJ";
    mkdir(tmp_dir, 0755);

    // 创建符号链接
    snprintf(symlink_path, sizeof(symlink_path), "%s/wrapper.node", tmp_dir);

    // 如果已存在，先删除
    unlink(symlink_path);

    if (symlink(original_path, symlink_path) != 0) {
        fprintf(stderr, "Failed to create symlink: %s\n", strerror(errno));
        return nullptr;
    }

    printf("Created bypass symlink: %s -> %s\n", symlink_path, original_path);
    return symlink_path;
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

    // 2. 尝试直接加载，如果失败则创建符号链接绕过检查
    const char* wrapper_path = "./wrapper.node";
    g_module = dlopen(wrapper_path, RTLD_LAZY);

    if (!g_module) {
        // 新版本可能需要 DFLJ 路径检查，创建符号链接
        printf("Direct load failed, trying bypass symlink...\n");

        // 查找原始 wrapper.node 路径
        char* bypass_path = create_bypass_symlink(wrapper_path);
        if (bypass_path) {
            g_module = dlopen(bypass_path, RTLD_LAZY);
            wrapper_path = bypass_path;
        }
    }

    if (!g_module) {
        fprintf(stderr, "dlopen wrapper.node failed: %s\n", dlerror());
        return -1;
    }

    // 3. 查找基地址
    g_base = 0;
    dl_iterate_phdr(find_module, (void*)wrapper_path);

    if (g_base == 0) {
        fprintf(stderr, "Failed to find wrapper.node base address\n");
        dlclose(g_module);
        g_module = nullptr;
        return -1;
    }

    // 4. 计算签名函数地址
    // 使用函数指针类型判断：
    // 新版本 (>= 0x50000000): 使用 sign_func_v2_t
    // 老版本 (< 0x50000000): 使用 sign_func_t
    // 注意：新版本 sub_57E1131 偏移约 92MB，老版本约 95MB，相近
    // 更可靠的判断：新版本偏移 0x57E1131 > 0x50000000

    uintptr_t func_addr = g_base + offset;
    printf("Sign function at: 0x%lx (base=0x%lx + offset=%lu)\n", func_addr, g_base, offset);

    // 根据编译时 SIGN_OFFSET 值决定版本（新版本 >= 0x50000000 即 52MB）
    #ifdef SIGN_OFFSET
    #if SIGN_OFFSET >= 50000000
        g_sign_func_v2 = (sign_func_v2_t)func_addr;
        printf("Using V2 signature function (sub_57E1131 style)\n");
    #else
        g_sign_func = (sign_func_t)func_addr;
        printf("Using V1 signature function\n");
    #endif
    #else
        // 默认使用 V1
        g_sign_func = (sign_func_t)func_addr;
        printf("Using V1 signature function (default)\n");
    #endif

    // 验证地址有效性
    if (func_addr < g_base) {
        fprintf(stderr, "Invalid function address\n");
        dlclose(g_module);
        g_module = nullptr;
        return -1;
    }

    return 0;
}

int sign_call(const char* cmd, const uint8_t* src, int src_len, int seq, uint8_t* out) {
    // 新版本函数
    if (g_sign_func_v2) {
        // sub_57E1131(module_id, data, data_len, seq, output)
        int ret = g_sign_func_v2(cmd, src, src_len, seq, out);
        return (ret == 0) ? 0 : -1;
    }

    // 老版本函数
    if (g_sign_func) {
        long ret = g_sign_func(cmd, src, src_len, seq, out);
        return (ret == 0) ? 0 : -1;
    }

    return -1;
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