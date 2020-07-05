using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.SMVim
{

  [Form(Mode = DefaultFields.None)]
  [Title("Dictionary Settings",
         IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
        "Cancel",
        IsCancel = true)]
  [DialogAction("save",
        "Save",
        IsDefault = true,
        Validates = true)]
  public class SMVimCfg : CfgBase<SMVimCfg>, INotifyPropertyChangedEx
  {

    /// <summary>
    /// Absolute path to neovim.exe
    /// TODO: document supported versions
    /// </summary>
    [Field(Name = "Neovim Path")]
    public string NeovimPath { get; set; }

    [JsonIgnore]
    public bool IsChanged { get; set; }

    public override string ToString() => "SMVim";

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
