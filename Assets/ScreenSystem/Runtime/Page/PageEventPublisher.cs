using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using ScreenSystem.Page.Messages;
using VContainer;

namespace ScreenSystem.Page
{
	public class PageEventPublisher : IDisposable
	{
		private readonly Channel<PagePushMessage> _pagePushChannel;
		private readonly Channel<PagePopMessage> _pagePopChannel;
		
		[Inject]
		public PageEventPublisher()
		{
			_pagePushChannel = Channel.CreateSingleConsumerUnbounded<PagePushMessage>();
			_pagePopChannel = Channel.CreateSingleConsumerUnbounded<PagePopMessage>();
		}

		public void SendPushEvent(IPageBuilder builder)
		{
			_pagePushChannel.Writer.TryWrite(new PagePushMessage(builder));
		}

		public void SendPopEvent(bool playAnimation = true)
		{
			_pagePopChannel.Writer.TryWrite(new PagePopMessage(playAnimation));
		}

		// イベントが飛んでこないためPublishは削除した
		public IUniTaskAsyncEnumerable<PagePushMessage> OnPagePushAsyncEnumerable()
			=> _pagePushChannel.Reader.ReadAllAsync();

		// Publishは削除した
		public IUniTaskAsyncEnumerable<PagePopMessage> OnPagePopAsyncEnumerable()
			=> _pagePopChannel.Reader.ReadAllAsync();

		public void Dispose()
		{
			_pagePushChannel.Writer.TryComplete();
			_pagePopChannel.Writer.TryComplete();
		}
	}
}