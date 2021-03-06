﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Base;

namespace Model
{
	public class LockInfo
	{
		public string Address;
		public TaskCompletionSource<bool> Tcs;

		public LockInfo(string address, TaskCompletionSource<bool> tcs)
		{
			this.Address = address;
			this.Tcs = tcs;
		}
	}

	[EntityEvent(typeof(MasterComponent))]
	public class MasterComponent : Component
	{
		/// 镜像的地址
		private readonly List<string> ghostsAddress = new List<string>();

		/// 当前获取锁的进程地址
		private string lockedAddress = "";

		/// 请求锁的队列
		private readonly Queue<LockInfo> queue = new Queue<LockInfo>();

		public void AddGhost(string address)
		{
			this.ghostsAddress.Add(address);
		}

		public void RemoveGhost(string address)
		{
			this.ghostsAddress.Remove(address);
		}

		public Task<bool> Lock(string address)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			
			if (this.lockedAddress == "")
			{
				this.lockedAddress = address;
				tcs.SetResult(true);
			}
			else
			{
				LockInfo lockInfo = new LockInfo(address, tcs);
				this.queue.Enqueue(lockInfo);
			}
			return tcs.Task;
		}

		public void Release(string address)
		{
			if (this.lockedAddress != address)
			{
				Log.Error($"解锁地址与锁地址不匹配! {this.lockedAddress} {address}");
				return;
			}
			if (this.queue.Count == 0)
			{
				this.lockedAddress = "";
				return;
			}
			LockInfo lockInfo = this.queue.Dequeue();
			this.lockedAddress = lockInfo.Address;
			lockInfo.Tcs.SetResult(true);
		}
	}
}