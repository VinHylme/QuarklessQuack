﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Quarkless.Filters
{
	internal class MaxConcurrentRequestsEnqueuer
	{
		public enum DropMode
		{
			Tail = MaxConcurrentRequestsLimitExceededPolicy.FifoQueueDropTail,
			Head = MaxConcurrentRequestsLimitExceededPolicy.FifoQueueDropHead
		}

		#region Fields
		private readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(1, 1);

		private readonly int _maxQueueLength;
		private readonly DropMode _dropMode;
		private readonly int _maxTimeInQueue;

		private readonly LinkedList<TaskCompletionSource<bool>> _queue = new LinkedList<TaskCompletionSource<bool>>();

		private static readonly Task<bool> _enqueueFailedTask = Task.FromResult(false);
		#endregion

		#region Constructor
		public MaxConcurrentRequestsEnqueuer(int maxQueueLength, DropMode dropMode, int maxTimeInQueue)
		{
			_maxQueueLength = maxQueueLength;
			_dropMode = dropMode;
			_maxTimeInQueue = maxTimeInQueue;
		}
		#endregion

		#region Methods
		public async Task<bool> EnqueueAsync(CancellationToken requestAbortedCancellationToken)
		{
			var enqueueTask = _enqueueFailedTask;

			if (_maxQueueLength <= 0) return await enqueueTask;
			var enqueueCancellationToken = GetEnqueueCancellationToken(requestAbortedCancellationToken);

			try
			{
				await _queueSemaphore.WaitAsync(enqueueCancellationToken);
				try
				{
					if (_queue.Count < _maxQueueLength)
					{
						enqueueTask = InternalEnqueueAsync(enqueueCancellationToken);
					}
					else if (_dropMode == DropMode.Head)
					{
						InternalDequeue(false);

						enqueueTask = InternalEnqueueAsync(enqueueCancellationToken);
					}
				}
				finally
				{
					_queueSemaphore.Release();
				}
			}
			catch (OperationCanceledException)
			{
			}

			return await enqueueTask;
		}

		public async Task<bool> DequeueAsync()
		{
			var dequeued = false;

			await _queueSemaphore.WaitAsync();
			try
			{
				if (_queue.Count > 0)
				{
					InternalDequeue(true);
					dequeued = true;
				}
			}
			finally
			{
				_queueSemaphore.Release();
			}

			return dequeued;
		}

		private Task<bool> InternalEnqueueAsync(CancellationToken enqueueCancellationToken)
		{
			Task<bool> enqueueTask = _enqueueFailedTask;

			if (enqueueCancellationToken.IsCancellationRequested) return enqueueTask;
			var enqueueTaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			enqueueCancellationToken.Register(CancelEnqueue, enqueueTaskCompletionSource);

			_queue.AddLast(enqueueTaskCompletionSource);
			enqueueTask = enqueueTaskCompletionSource.Task;

			return enqueueTask;
		}

		private CancellationToken GetEnqueueCancellationToken(CancellationToken requestAbortedCancellationToken)
		{
			var enqueueCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
				requestAbortedCancellationToken,
				GetTimeoutToken()).Token;

			return enqueueCancellationToken;
		}

		private CancellationToken GetTimeoutToken()
		{
			var timeoutToken = CancellationToken.None;

			if (_maxTimeInQueue == MaxConcurrentRequestsOptions.MaxTimeInQueueUnlimited) return timeoutToken;
			CancellationTokenSource timeoutTokenSource = new CancellationTokenSource();

			timeoutToken = timeoutTokenSource.Token;

			timeoutTokenSource.CancelAfter(_maxTimeInQueue);

			return timeoutToken;
		}

		private void CancelEnqueue(object state)
		{
			bool removed = false;

			TaskCompletionSource<bool> enqueueTaskCompletionSource = ((TaskCompletionSource<bool>)state);

			// This is blocking, but it looks like this callback can't be asynchronous.
			_queueSemaphore.Wait();
			try
			{
				removed = _queue.Remove(enqueueTaskCompletionSource);
			}
			finally
			{
				_queueSemaphore.Release();
			}

			if (removed)
			{
				enqueueTaskCompletionSource.SetResult(false);
			}
		}

		private void InternalDequeue(bool result)
		{
			TaskCompletionSource<bool> enqueueTaskCompletionSource = _queue.First.Value;

			_queue.RemoveFirst();

			enqueueTaskCompletionSource.SetResult(result);
		}
		#endregion
	}
}
