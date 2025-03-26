// src/components/InvoiceDetails/InvoiceDetails.jsx
import { motion } from "framer-motion";
import styles from "./InvoiceDetails.module.css";
import {
  FaRegCalendarAlt,
  FaRegMoneyBillAlt,
  FaHashtag,
  FaBuilding,
  FaListUl,
} from "react-icons/fa";

const InvoiceDetails = ({ invoiceData }) => {
  if (!invoiceData) return null;

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, delay: 0.2 }}
      className={styles.detailsContainer}
    >
      <h2 className={styles.title}>Invoice Details</h2>

      <div className={styles.detailsGrid}>
        <div className={styles.detailCard}>
          <div className={styles.detailHeader}>
            <FaBuilding className={styles.detailIcon} />
            <h3>Vendor</h3>
          </div>
          <p className={styles.detailValue}>{invoiceData.vendor || "N/A"}</p>
        </div>

        <div className={styles.detailCard}>
          <div className={styles.detailHeader}>
            <FaRegCalendarAlt className={styles.detailIcon} />
            <h3>Date</h3>
          </div>
          <p className={styles.detailValue}>{invoiceData.date || "N/A"}</p>
        </div>

        <div className={styles.detailCard}>
          <div className={styles.detailHeader}>
            <FaHashtag className={styles.detailIcon} />
            <h3>Invoice #</h3>
          </div>
          <p className={styles.detailValue}>
            {invoiceData.invoiceNumber || "N/A"}
          </p>
        </div>

        <div className={styles.detailCard}>
          <div className={styles.detailHeader}>
            <FaRegMoneyBillAlt className={styles.detailIcon} />
            <h3>Total</h3>
          </div>
          <p className={styles.detailValue}>{invoiceData.total || "N/A"}</p>
        </div>
      </div>

      {invoiceData.lineItems && invoiceData.lineItems.length > 0 && (
        <div className={styles.lineItems}>
          <div className={styles.lineItemsHeader}>
            <FaListUl className={styles.detailIcon} />
            <h3>Line Items</h3>
          </div>
          <div className={styles.lineItemsTable}>
            <div className={styles.tableHeader}>
              <div>Description</div>
              <div>Quantity</div>
              <div>Price</div>
              <div>Total</div>
            </div>
            {invoiceData.lineItems.map((item, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ duration: 0.3, delay: index * 0.1 }}
                className={styles.tableRow}
              >
                <div>{item.description || "N/A"}</div>
                <div>{item.quantity || "N/A"}</div>
                <div>{item.price || "N/A"}</div>
                <div>{item.total || "N/A"}</div>
              </motion.div>
            ))}
          </div>
        </div>
      )}
    </motion.div>
  );
};

export default InvoiceDetails;
