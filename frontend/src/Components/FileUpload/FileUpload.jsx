import { useCallback, useState } from "react";
import { useDropzone } from "react-dropzone";
import { motion } from "framer-motion";
import axios from "axios";
import styles from "./FileUpload.module.css";
import { FiUpload, FiImage, FiX } from "react-icons/fi";

const FileUpload = ({ onUploadSuccess }) => {
  const [file, setFile] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const onDrop = useCallback((acceptedFiles) => {
    if (acceptedFiles.length > 0) {
      const selectedFile = acceptedFiles[0];
      if (
        selectedFile.type === "image/png" ||
        selectedFile.type === "image/jpeg"
      ) {
        setFile(selectedFile);
        setError(null);
      } else {
        setError("Please upload PNG or JPEG image.");
      }
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      "image/png": [".png"],
      "image/jpeg": [".jpeg", ".jpg"],
    },
    maxFiles: 1,
  });

  const removeFile = () => {
    setFile(null);
  };

  const uploadFile = async () => {
    if (!file) return;

    setIsLoading(true);
    setError(null);

    const formData = new FormData();
    formData.append("invoice", file);

    try {
      const response = await axios.post(
        "http://localhost:5000/api/OCR/upload",
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      );

      // Handle the response (e.g., display extracted text)
      const extractedText = response.data.extractedText;
      console.log("Extracted Text:", extractedText);
      onUploadSuccess(extractedText);
    } catch (err) {
      setError(err.response?.data || "Failed to upload file");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
      className={styles.uploadContainer}
    >
      <h2 className={styles.title}>Upload Invoice</h2>
      <p className={styles.subtitle}>
        Drag & drop a PNG invoice or click to browse
      </p>

      <div
        {...getRootProps()}
        className={`${styles.dropzone} ${isDragActive ? styles.active : ""}`}
      >
        <input {...getInputProps()} />
        {file ? (
          <div className={styles.filePreview}>
            <FiImage className={styles.fileIcon} />
            <span className={styles.fileName}>{file.name}</span>
            <button onClick={removeFile} className={styles.removeButton}>
              <FiX />
            </button>
          </div>
        ) : (
          <div className={styles.dropContent}>
            <FiUpload className={styles.uploadIcon} />
            <p>
              {isDragActive
                ? "Drop the file here"
                : "Select or drag a file here"}
            </p>
          </div>
        )}
      </div>

      {error && <p className={styles.error}>{error}</p>}

      {file && (
        <motion.button
          whileHover={{ scale: 1.03 }}
          whileTap={{ scale: 0.98 }}
          onClick={uploadFile}
          disabled={isLoading}
          className={styles.uploadButton}
        >
          {isLoading ? "Processing..." : "Extract Invoice Data"}
        </motion.button>
      )}
    </motion.div>
  );
};

export default FileUpload;
