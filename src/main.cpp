#include "sign.h"
#include "httplib.h"
#include "nlohmann/json.hpp"
#include <iostream>
#include <vector>
#include <cstring>

using json = nlohmann::json;

// 签名函数偏移地址 - 适配不同版本 Lagrange.Milky
#define SIGN_OFFSET 0x5ADE220

int main() {
    // 初始化：预加载依赖库
    const char* libs[] = {
        "libgnutls.so.30",
        "./libsymbols.so"
    };

    if (sign_init(libs, 2, SIGN_OFFSET) != 0) {
        std::cerr << "Failed to load wrapper.node" << std::endl;
        return 1;
    }

    std::cout << "CharonAnchor started on http://127.0.0.1:8080" << std::endl;

    // HTTP 服务
    httplib::Server svr;

    svr.Post("/", [](const httplib::Request& req, httplib::Response& res) {
        try {
            json body = json::parse(req.body);

            std::string cmd = body["cmd"];
            std::string src_hex = body["src"];
            int seq = body["seq"];

            // hex -> bytes
            std::vector<uint8_t> src;
            for (size_t i = 0; i < src_hex.length(); i += 2) {
                uint8_t byte = std::stoi(src_hex.substr(i, 2), nullptr, 16);
                src.push_back(byte);
            }

            // 调用签名函数
            uint8_t out_buf[0x300] = {0};
            if (sign_call(cmd.c_str(), src.data(), src.size(), seq, out_buf) != 0) {
                res.status = 500;
                res.set_content(json{{"error", "sign failed"}}.dump(), "application/json");
                return;
            }

            // 提取结果
            uint8_t token[256], extra[256], sign_out[256];
            int token_len, extra_len, sign_len;
            sign_extract(out_buf, token, &token_len, extra, &extra_len, sign_out, &sign_len);

            // bytes -> hex
            auto to_hex = [](const uint8_t* data, int len) {
                std::string hex;
                for (int i = 0; i < len; i++) {
                    char buf[3];
                    snprintf(buf, 3, "%02X", data[i]);
                    hex += buf;
                }
                return hex;
            };

            json response = {
                {"value", {
                    {"token", to_hex(token, token_len)},
                    {"extra", to_hex(extra, extra_len)},
                    {"sign", to_hex(sign_out, sign_len)}
                }}
            };

            res.set_content(response.dump(), "application/json");

        } catch (const std::exception& e) {
            res.status = 400;
            res.set_content(json{{"error", e.what()}}.dump(), "application/json");
        }
    });

    svr.listen("127.0.0.1", 8080);

    sign_cleanup();
    return 0;
}