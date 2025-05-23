using System.Text;

using Net.Codecrete.QrCodeGenerator;

namespace Lagrange.Core.Runner.Utility;

public static class QrCodeUtility
{
    public static string GenerateAscii(string payload, bool compatible)
    {
        QrCode qrcode = QrCode.EncodeText(payload, QrCode.Ecc.Low);

        StringBuilder result = new();
        for (int y = 0; y < qrcode.Size; y += 2)
        {
            for (int x = 0; x < qrcode.Size; x++)
            {
                bool top = qrcode.GetModule(x, y);
                bool bottom = qrcode.GetModule(x, y + 1);

                result.Append((top, bottom) switch
                {
                    (true, true) => compatible ? '@' : '█',
                    (true, false) => compatible ? '^' : '▀',
                    (false, true) => compatible ? '.' : '▄',
                    (false, false) => compatible ? ' ' : ' ',
                });
            }
            if (y < qrcode.Size) result.Append('\n');
        }

        return result.ToString();
    }
}