#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   7/5/2020 1:12:31 AM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.SMVim
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Runtime.Remoting;
  using Anotar.Serilog;
  using HtmlAgilityPack;
  using HTMLControlEvents;
  using mshtml;
  using NeovimClient;
  using SMAUsefulFunctions;
  using SuperMemoAssistant.Interop.SuperMemo.Core;
  using SuperMemoAssistant.Services;
  using SuperMemoAssistant.Services.Sentry;
  using SuperMemoAssistant.Sys.Remoting;

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class SMVimPlugin : SentrySMAPluginBase<SMVimPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public SMVimPlugin() : base("Enter your Sentry.io api key (strongly recommended)") { }

    #endregion

    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "SMVim";

    /// <inheritdoc />
    public override bool HasSettings => false;
    public SMVimCfg Config { get; set; }

    /// <summary>
    /// The headless, embedded nvim process
    /// </summary>
    public NeovimClient<NeovimHost> nvim { get; private set; }

    /// <summary>
    /// Convenience class for subscribing to IHtmlControl events
    /// </summary>
    public HTMLControlEvents HtmlDocEvents { get; set; }

    /// <summary>
    /// The number of buffers to create in nvim to hold SM component content
    /// </summary>
    private const int NVIM_BUFFERS = 10;

    #endregion

    #region Methods Impl

    private void LoadConfig()
    {
      Config = Svc.Configuration.Load<SMVimCfg>() ?? new SMVimCfg();
    }

    /// <inheritdoc />
    protected override void PluginInit()
    {
      
      LoadConfig();

      CreateNvimHost();

      CreateBuffers();

      nvim.AttachToUI();

      SubscribeToHtmlDocEvents();

      Svc.SM.UI.ElementWdw.OnElementChanged += new ActionProxy<SMDisplayedElementChangedEventArgs>(OnElementChanged);

    }

    // TODO: Find an event that fires when html control is focused
    private void SubscribeToHtmlDocEvents()
    {

      var options = new List<EventInitOptions>
      {
        new EventInitOptions(EventType.onkeydown, _ => true, x => ((IHTMLElement)x).tagName.ToLower() == "body"),

        // TODO: Check this works
        new EventInitOptions(EventType.onactivate, _ => true, x => ((IHTMLElement)x).tagName.ToLower() == "body")
      };

      HtmlDocEvents = new HTMLControlEvents(options);

      HtmlDocEvents.OnKeyDownEvent += HtmlDocOnKeyDownEventHandler;
      HtmlDocEvents.OnActivateEvent += HtmlDocOnActivateEventHandler;

    }

    private void HtmlDocOnActivateEventHandler(object sender, IHTMLControlEventArgs e)
    {
      nvim.SetCurrentBuffer(e.ControlIdx);
      // TODO: Need to reset cursor position??
    }

    /// <summary>
    /// OnKeyDown event fires before the input is sent to the IHtmlControl
    /// This handler intercepts the event and sends the keycode to nvim.
    /// SM does not recieve this key.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HtmlDocOnKeyDownEventHandler(object sender, IHTMLControlEventArgs e)
    {
      
      var eventObj = e.EventObj;

      // This prevents the event propagating to keypress / keyup etc.
      eventObj.returnValue = false;

      // TODO: This is almost certainly wrong.
      // TODO: Look into defaulCharset mshtml.
      string key = ((char)eventObj.keyCode).ToString().ToLower();

      // send the input to the current nvim buffer
      nvim.SendKeys(key);

    }

    /// <summary>
    /// Create the embedded, headless Nvim host process and subscribe to recieve events
    /// </summary>
    private void CreateNvimHost()
    {

      nvim = new NeovimClient<NeovimHost>(new NeovimHost(Config.NeovimPath));
      nvim.Init();
      nvim.NotificationReceived += NvimEventHandler;

    }

    private void ClearBuffers()
    {

      for (int i = 0; i < NVIM_BUFFERS; i++)
      {
        nvim.ClearBuffer(i);
      }

    }


    /// <summary>
    /// Create nvim buffers to hold SM control content.
    /// SM components are mapped by their control index to the corresponding nvim buffer.
    /// Each buffer is cleared and reloaded on element changed event.
    /// </summary>
    private void CreateBuffers()
    {

      for (int i = 0; i < NVIM_BUFFERS; i++)
      {
        nvim.CreateBuffer();
      }

    }


    /// <summary>
    /// Loads each nvim buffer with corresponding IHtmlControl content.
    /// </summary>
    private void LoadBuffersWithSMContent()
    {

      // TODO: cross assembly type issue
      // Dictionary<int, IHTMLDocument2> htmlDocs = ElementContent.GetAllHtmlDocuments();
      Dictionary<int, IHTMLDocument2> htmlDocs = new Dictionary<int, IHTMLDocument2>();
      foreach (var htmlDoc in htmlDocs)
      {
        nvim.SetCurrentBuffer(htmlDoc.Key);
        nvim.SetBufferContent(GetInnerText(htmlDoc.Value?.body?.innerText));
      }

      nvim.SetCurrentBuffer(0);

    }

    private string GetInnerText(string html)
    {

      if (html.IsNullOrEmpty())
        return string.Empty;

      var doc = new HtmlDocument();
      doc.LoadHtml(html);
      return doc.DocumentNode.InnerText;

    }

    private void NvimEventHandler(object sender, NeovimNotificationEventArgs e)
    {
      // Redraw event sends information about where to place the cursor
      // I think it sends content / deletion information as well
      if (e.Name == "redraw")
      {
        // Update the content / cursor position in the focused HtmlControl
      }
    }

    #endregion

    #region Methods

    [LogToErrorOnException]
    public void OnElementChanged(SMDisplayedElementChangedEventArgs e)
    {
      try
      {

        // Unsub from redraw events while buffers get reloaded
        nvim.DetachFromUI();

        ClearBuffers();

        LoadBuffersWithSMContent();

        // Resub to the redraw events
        nvim.AttachToUI();

      }
      catch (RemotingException) { }
    }

    #endregion
  }
}
