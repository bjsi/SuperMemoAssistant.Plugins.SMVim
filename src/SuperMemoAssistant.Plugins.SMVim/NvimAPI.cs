using MsgPack;
using NeovimClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.SMVim
{

  public static class NvimAPI
  {

    public static long SendKeys(this NeovimClient<NeovimHost> nvim, string keys)
    {
      return nvim.Func<string, long>("nvim_input", keys);
    }

    public static Tuple<long, long> GetCursorPos(this NeovimClient<NeovimHost> nvim)
    {
      long[] ret = nvim.Func<long, long[]>("nvim_win_get_cursor", 0);
      return new Tuple<long, long>(ret[0], ret[1]);
    }

    // TODO: This is probably wrong.
    public static string GetContent(this NeovimClient<NeovimHost> nvim)
    {
      string[] content = nvim.Func<long, long, long, bool, string[]>("nvim_buf_get_lines", 0, 0, -1, true);
      return string.Join("\n", content);
    }

    public static void AttachToUI(this NeovimClient<NeovimHost> nvim)
    {
      nvim.Action<long, long, bool>("ui_attach", 100, 100, false);
    }

    public static bool AttachToBuffer(this NeovimClient<NeovimHost> nvim)
    {
      return nvim.Func<long, bool, MessagePackObject, bool>("nvim_buf_attach", 0, false, new MessagePackObject(new MessagePackObjectDictionary()));
    }

    // TODO:
    public static void CreateBuffer(this NeovimClient<NeovimHost> nvim)
    {

    }

    public static void DetachFromUI(this NeovimClient<NeovimHost> nvim)
    {
      nvim.Action("nvim_ui_detach");
    }

    public static void ClearBuffer(this NeovimClient<NeovimHost> nvim, long bufId)
    {
      nvim.Action<long, long, long, bool, string[]>("nvim_buf_set_lines", 0, 0, -1, true, new string[] { "" });
    }

    public static void SetBufferContent(this NeovimClient<NeovimHost> nvim, string[] content)
    {
      nvim.Action<long, long, long, bool, string[]>("nvim_buf_set_lines", 0, 0, 0, false, content);
    }

  }
}
