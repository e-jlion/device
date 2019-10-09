﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WP.Device.Framework;

namespace WP.Device.Framework
{
    public class MouseHookPlugins
    {
        #region 定义
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;


        /// <summary>
        /// 坐标点
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// 钩子结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private class MouseHookStruct
        {
            public POINT pt;
            public int hWnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        /// <summary>
        /// 钩子回调函数
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        // 声明鼠标钩子事件类型
        private BaseWin32Api.HookProcess _mouseHookProcedure;
        private static int _hMouseHook = 0; //鼠标钩子句柄

        // 全局的鼠标事件
        public event MouseEventHandler OnMouseActivity;
        #endregion

        #region 构造和析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public MouseHookPlugins()
        {
        }
        
        /// <summary>
        /// 析构函数
        /// </summary>
        ~MouseHookPlugins()
        {
            Stop();
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 启动全局钩子
        /// </summary>
        public void Start()
        {
            // 安装鼠标钩子
            if (_hMouseHook == 0)
            {
                // 生成一个HookProc的实例.
                _mouseHookProcedure = new BaseWin32Api.HookProcess(MouseHookProc);

                ProcessModule cModule = Process.GetCurrentProcess().MainModule;
                var mhIntPrt = BaseWin32Api.GetModuleHandle(cModule.ModuleName);

                _hMouseHook = BaseWin32Api.SetWindowsHookEx(ConstDefintion.WH_MOUSE_LL, _mouseHookProcedure, mhIntPrt, 0);

                //假设装置失败停止钩子
                if (_hMouseHook == 0)
                {
                    Stop();
                    throw new Exception("安装鼠标钩子失败...");
                }
            }
        }

        /// <summary>
        /// 停止全局钩子
        /// </summary>
        public void Stop()
        {
            bool retMouse = true;

            if (_hMouseHook != 0)
            {
                retMouse = BaseWin32Api.UnhookWindowsHookEx(_hMouseHook);
                _hMouseHook = 0;
            }

            // 假设卸下钩子失败
            if (!(retMouse))
                throw new Exception("UnhookWindowsHookEx failed.");
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 鼠标钩子回调函数
        /// </summary>
        private int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            // 假设正常执行而且用户要监听鼠标的消息
            if ((nCode >= 0) && (OnMouseActivity != null))
            {
                MouseButtons button = MouseButtons.None;
                int clickCount = 0;

                switch (wParam)
                {
                    case WM_LBUTTONDOWN:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case WM_LBUTTONUP:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case WM_LBUTTONDBLCLK:
                        button = MouseButtons.Left;
                        clickCount = 2;
                        break;
                    case WM_RBUTTONDOWN:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case WM_RBUTTONUP:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case WM_RBUTTONDBLCLK:
                        button = MouseButtons.Right;
                        clickCount = 2;
                        break;
                }
                
                // 从回调函数中得到鼠标的信息
                MouseHookStruct mouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                MouseEventArgs e = new MouseEventArgs(button, clickCount, mouseHookStruct.pt.x, mouseHookStruct.pt.y, 0);

                //// 假设想要限制鼠标在屏幕中的移动区域能够在此处设置
                //// 后期须要考虑实际的x、y的容差
                //if (!Screen.PrimaryScreen.Bounds.Contains(e.X, e.Y))
                //{
                //    //return 1;
                //}

                OnMouseActivity(this, e);
            }

            //启动下一次钩子
            return BaseWin32Api.CallNextHookEx(_hMouseHook, nCode, wParam, lParam);
        }
        #endregion
    }
}