// src/pages/HomePage.jsx
import { useState } from "react";
import FileUpload from "../Components/FileUpload/FileUpload";
import InvoiceDetails from "../Components/InvoiceDetails/InvoiceDetails";
import styles from "./HomePage.module.css";

const HomePage = () => {
  const [invoiceData, setInvoiceData] = useState(null);

  const handleUploadSuccess = (data) => {
    setInvoiceData(data);
  };

  return (
    <div className={styles.container}>
      <div className={styles.content}>
        <FileUpload onUploadSuccess={handleUploadSuccess} />
        {invoiceData && <InvoiceDetails invoiceData={invoiceData} />}
      </div>
    </div>
  );
};

export default HomePage;
