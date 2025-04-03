import React, { useState } from 'react';
import axios from 'axios';

function App() {
  const [file, setFile] = useState(null);
  const [extractedText, setExtractedText] = useState('');
  const [loading, setLoading] = useState(false);

  const handleFileChange = (e) => {
    setFile(e.target.files[0]);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    const formData = new FormData();
    formData.append('file', file);

    try {
      const response = await axios.post('http://localhost:5000/extract', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      setExtractedText(response.data.text);
    } catch (error) {
      console.error('Error:', error);
    }
    setLoading(false);
  };

  return (
    <div className="container">
      <h1>PDF Text Extractor</h1>
      <form onSubmit={handleSubmit}>
        <input 
          type="file" 
          accept=".pdf"
          onChange={handleFileChange}
        />
        <button type="submit" disabled={!file || loading}>
          {loading ? 'Extracting...' : 'Extract Text'}
        </button>
      </form>
      {extractedText && (
        <div className="result">
          <h2>Extracted Text:</h2>
          <pre>{extractedText}</pre>
        </div>
      )}
    </div>
  );
}

export default App;