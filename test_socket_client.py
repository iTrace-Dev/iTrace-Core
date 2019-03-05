# Simply python script to test the socket output
# Python 2.7 version (using 3.0, just parenthesize the print functions)

# NOTE:
# This demo does not properly handle losing connection to the client when
#  tracking stops. Just use Ctrl+C to kill the program or close terminal window.

import socket
import sys

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Connect to iTrace-Core
server_address = ('localhost', 8008)
print ('Attempt connection to iTrace socket')

# Continually Attempt Connection (wait for server to be ready) 
while sock.connect_ex(server_address):
        pass

print ('Connected!')

# Get the data and print to the screen
try:
	while True:
		print (sock.recv(1024))

finally:
	print ('Close')
	sock.close()
		
