const { contextBridge, ipcMain } = require('electron');

contextBridge.exposeInMainWorld('electronAPI', {
  send: (channel, data) => {
    ipcMain.send(channel, data);
  },
  receive: (channel, func) => {
    ipcMain.on(channel, (event, ...args) => func(...args));
  }
});
