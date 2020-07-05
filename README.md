# SMVim Notes
In short, this plugin aims to implement a true, integrated vim mode within SuperMemo IHtmlControls.
## Implementation 
- The integration will be achieved through an SMA plugin that can communicate with a headless, embedded Neovim instance via the [Nvim API](https://neovim.io/doc/user/api.html).
- The contents of IHtmlControls on the currently displayed element will be mapped to separate Nvim buffers. User keyboard input within an IHtmlControl will be intercepted by the SMA plugin and redirected to the corresponding Nvim buffer.
-  The Nvim instance will return events specifying how to update the IHtmlControl content and cursor position in response to the intercepted keypresses.
- The contents of buffers will be cleared and reloaded in response to the SMA displayed element changed event.

## Technical Notes
- There are two .NET libraries that allow you to interact with Nvim processes via the Nvim API - [nvim.net](https://github.com/neovim/nvim.net) and [NeovimClient](https://github.com/dalance/NeovimClient).
- I ran into issues with both of these libraries:
	- With nvim.net I kept getting errors when receiving events.
	- NeovimClient seems to work better despite being older but some events weren't working depending on the version of Nvim I used.
	- (It's possible I have been doing something wrong)

## Other
- I am working on a [library]([https://github.com/bjsi/SuperMemoAssistant.HtmlControlEvents](https://github.com/bjsi/SuperMemoAssistant.HtmlControlEvents)) to simplify subscribing to IHtmlControl events. I included it in this library as a submodule but I haven't tested it thoroughly yet, or worked out which events work.
	+ I need to find some event that fires when the focused control changes to know when and where to set the active Nvim buffer.
