﻿using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WallpaperSetter
{
    /// <summary>壁紙の設定を行うためのクラス</summary>
    /// http://smdn.jp/programming/tips/setdeskwallpaper/
    static class Wallpaper
    {
        /// <summary>壁紙を設定する</summary>
        /// <param name="file">壁紙として設定する画像ファイル</praram>
        public static void SetWallpaper(string file, WallpaperStyle style)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("ファイル名が空", "file");
            if (style == null)
                throw new ArgumentNullException("style");

            // HKEY_CURRENT_USER\Control Panel\Desktopを開く
            using (var regkeyDesktop = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (regkeyDesktop == null)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // TileWallpaperとWallpaperStyleを設定
                regkeyDesktop.SetValue("TileWallpaper", style._TileWallpaper.ToString());
                regkeyDesktop.SetValue("WallpaperStyle", style._WallpaperStyle.ToString());

                // 「並べて表示」、「拡大して表示」などの原点も変えたい場合は、この値を0以外に変更する
                regkeyDesktop.SetValue("WallpaperOriginX", "0");
                regkeyDesktop.SetValue("WallpaperOriginY", "0");

                // Wallpaperの値をセットすることでも壁紙を設定できるが、
                // SystemParametersInfoを呼び出さないと、壁紙を設定しても即座には反映されない
                //regkeyDesktop.SetValue("Wallpaper", file);
            }

            SetDeskWallpaper(file);
        }

        /// <summary>壁紙をはがす</summary>
        public static void UnsetWallpaper()
        {
            // ファイル名に"\0"を指定すると、
            // 壁紙をはがす(背景色のみの状態にする)ことができる
            SetDeskWallpaper("\0");
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction,
                                                        int uParam,
                                                        string lpvParam,
                                                        int fuWinIini);

        private static void SetDeskWallpaper(string file)
        {
            const int SPI_SETDESKWALLPAPER = 0x0014; // デスクトップの壁紙を設定
            const int SPIF_UPDATEINIFILE = 0x0001; // 設定を更新する
            const int SPIF_SENDWININICHANGE = 0x0002; // 設定の更新を全てのアプリケーションに通知(WM_SETTIMGCHANGE)する

            //var flags = (Environment.OSVersion.Platform == PlatformID.Win32NT)
            //  ? SPIF_SENDWININICHANGE
            //  : SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE;
            var flags = SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE;

            // 壁紙となるファイルを設定する
            // なお、XP以前の場合はBMP形式のみが指定可能、Vista/7では加えてJPEG形式も指定可能
            if (!SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, file, flags))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
