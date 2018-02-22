# Will need to install websocket package
# use: pip install websocket-client

from websocket import create_connection

ws = create_connection("ws://localhost:7007")
print 'Connected to iTrace WebSocket'

while (True):
	print ws.recv()

ws.close()
