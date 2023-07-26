### 一些测试读写分离和高可用主从切换的命令：

- 开启监视器，将监听到的操作写入到文件（在每个redis节点执行）

  ```shell
  redis-cli -a bb123456 MONITOR > /data/logs.txt
  ```

- 查看该redis节点执行过的get命令（在从节点执行，用于区分是否读写分离和负载均衡的情况）

  ```shell
  cat logs.txt | grep "GET"
  ```

- 统计该redis节点执行get命令的次数

  ```shell
  cat logs.txt | grep "GET" | wc -l
  ```

- 模拟redis节点宕机（主节点执行）

  ```shell
  redis-cli -a bb123456 debug sleep 50
  ```

  

