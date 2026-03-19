window.downloadFile = (filename, base64) => {

    const link = document.createElement('a');

    link.href = "data:text/csv;base64," + base64;

    link.download = filename;

    link.click();
};

