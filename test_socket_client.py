# Echo client program
import socket
import sys

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Connect
server_address = ('localhost', 8008)
print 'connecting to server'
sock.connect(server_address)


try:
	while True:
		data = sock.recv(1024)
		print data

finally:
	print 'close'
	sock.close()
		