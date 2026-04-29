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
        "./libsymbols.so",
        "./libbugly.so",
        "./libcrbase.so"
    };

    if (sign_init(libs, 4, SIGN_OFFSET) != 0) {
        std::cerr << "Failed to load wrapper.node" << std::endl;
        return 1;
    }

    std::cout << "CharonAnchor started on http://127.0.0.1:8080" << std::endl;

    // HTTP 服务
    httplib::Server svr;

    // Lagrange.Milky 期望的签名路径
    svr.Post("/api/sign/sec-sign", [](const httplib::Request& req, httplib::Response& res) {
        try {
            json body = json::parse(req.body);

            std::string command = body["command"];
            std::string body_hex = body["body"];
            int seq = body["seq"];

            // hex -> bytes
            std::vector<uint8_t> src;
            for (size_t i = 0; i < body_hex.length(); i += 2) {
                uint8_t byte = std::stoi(body_hex.substr(i, 2), nullptr, 16);
                src.push_back(byte);
            }

            // 调用签名函数
            uint8_t out_buf[0x300] = {0};
            if (sign_call(command.c_str(), src.data(), src.size(), seq, out_buf) != 0) {
                json response = {
                    {"code", -1},
                    {"message", "sign failed"},
                    {"value", {
                        {"sec_sign", ""},
                        {"sec_token", ""},
                        {"sec_extra", ""}
                    }}
                };
                res.set_content(response.dump(), "application/json");
                return;
            }

            // 提取结果
            uint8_t token[256], extra[256], sign_out[256];
            int token_len, extra_len, sign_len;
            sign_extract(out_buf, token, &token_len, extra, &extra_len, sign_out, &sign_len);

            // bytes -> hex (小写)
            auto to_hex = [](const uint8_t* data, int len) {
                std::string hex;
                for (int i = 0; i < len; i++) {
                    char buf[3];
                    snprintf(buf, 3, "%02x", data[i]);
                    hex += buf;
                }
                return hex;
            };

            json response = {
                {"code", 0},
                {"message", nullptr},
                {"value", {
                    {"sec_sign", to_hex(sign_out, sign_len)},
                    {"sec_token", to_hex(token, token_len)},
                    {"sec_extra", to_hex(extra, extra_len)}
                }}
            };

            res.set_content(response.dump(), "application/json");

        } catch (const std::exception& e) {
            json response = {
                {"code", -2},
                {"message", e.what()},
                {"value", {
                    {"sec_sign", ""},
                    {"sec_token", ""},
                    {"sec_extra", ""}
                }}
            };
            res.set_content(response.dump(), "application/json");
        }
    });

    svr.listen("127.0.0.1", 8080);

    sign_cleanup();
    return 0;
}