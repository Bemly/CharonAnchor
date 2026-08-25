# MSF-NG 攻坚交接文档

> 生成：2026-08-25 深夜。交接对象：下一个 agent。
> 配套长期记忆：`memory/project_msfng_transport.md`（本文件的母本，含全部历史细节）。
>
> ⚡⚡ **2026-08-25 深夜更新：传输层已打通 + 签名绑定成最后堵点！**
> 官方 trans_emp 帧完全破解（TEA 链式 = Lagrange pre-XOR 变体，非标准 CBC；wrapper 填充
> pad=(5-len)&7 无尾部）。重放官方帧 → 服务器 0.1s 回 927B retCode=0（多次复现）。
> 隔离实验证明：secSig 绑定请求内容，验签不过=静默。本地 wrapper 能算 trans_emp 签名（不挂起），
> 但精确输入绑定未命中 —— **下一动作：动态 Hook 官方客户端签名函数拿精确参数**（行动清单见
> memory/project_msfng_transport.md 最底部）。注意 busi = wrapper 内部 0x02 载荷（gdb 模板即此格式），
> 不是经典 code2d TLV；Milky 需换 WrapperTeaEncrypt（填充不同）。