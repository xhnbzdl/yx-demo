﻿lock (Monitor)
作用范围：仅限于同一个进程内的线程同步。
实现方式：语法糖，实质上是 Monitor.Enter 和 Monitor.Exit 的简化。
性能：由于是轻量级锁，在大多数情况下性能优于 Mutex。
使用场景：适用于单进程内的线程同步，是最常用的同步机制之一。


Mutex
作用范围：可以用于跨进程同步。
实现方式：依赖于系统内核对象。
性能：由于涉及内核对象的创建和销毁，性能较 lock 稍差。
使用场景：适用于在同一个服务器上需要跨进程同步的情况，如多个应用程序需要访问同一个资源。需要手动管理锁的获取和释放，适合复杂的同步场景.


为什么 lock 会阻塞线程，而 SpinLock 不会
阻塞线程：当我们说 lock 会阻塞线程时，指的是线程在等待锁时会被挂起，这意味着它不会占用 CPU 资源。操作系统会将该线程置于等待状态，并在锁可用时将其唤醒。
自旋等待：SpinLock 不会阻塞线程，因为它不会将线程置于等待状态。相反，线程会继续运行并不断检查锁的状态。这意味着线程在等待锁时会占用 CPU 资源。