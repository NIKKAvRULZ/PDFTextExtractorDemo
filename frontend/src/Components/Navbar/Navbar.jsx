import { motion } from "framer-motion";
import styles from "./Navbar.module.css";
import { FaFileInvoiceDollar } from "react-icons/fa";

const Navbar = () => {
  return (
    <motion.nav
      initial={{ y: -100 }}
      animate={{ y: 0 }}
      transition={{ duration: 0.5 }}
      className={styles.navbar}
    >
      <div className={styles.logoContainer}>
        <FaFileInvoiceDollar className={styles.logo} />
        <span className={styles.logoText}>InvoiceAI</span>
      </div>

      <div className={styles.navLinks}>
        <a href="#" className={styles.navLink}>
          Home
        </a>
        <a href="#" className={styles.navLink}>
          History
        </a>
        <a href="#" className={styles.navLink}>
          About
        </a>
      </div>
    </motion.nav>
  );
};

export default Navbar;
