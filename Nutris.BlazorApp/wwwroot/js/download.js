<script>
  window.downloadFileBase64 = (fileName, contentType, base64Data) => {
    const a = document.createElement('a');
    a.href = `data:${contentType};base64,${base64Data}`;
    a.download = fileName || 'download';
    a.style.display = 'none';
    document.body.appendChild(a);
    a.click();
    a.remove();
  };


</script>
