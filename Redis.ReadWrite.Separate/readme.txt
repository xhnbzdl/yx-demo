һЩ���Զ�д����͸߿��������л������
# ���������������������Ĳ���д�뵽�ļ�����ÿ��redis�ڵ�ִ�У�
redis-cli -a bb123456 MONITOR > /data/logs.txt
# �鿴��redis�ڵ�ִ�й���get����ڴӽڵ�ִ�У����������Ƿ��д����͸��ؾ���������
cat logs.txt | grep "GET"
# ͳ�Ƹ�redis�ڵ�ִ��get����Ĵ���
cat logs.txt | grep "GET" | wc -l
# ģ��redis�ڵ�崻������ڵ�ִ�У�
redis-cli -a bb123456 debug sleep 50