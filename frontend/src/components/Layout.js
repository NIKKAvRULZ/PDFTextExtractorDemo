import React from 'react';
import './Layout.css'; // Assuming you have some styles for the layout

const Layout = ({ children }) => {
    return (
        <div className="layout">
            <header className="layout-header">
                <h1>My Application</h1>
                {/* You can add navigation links here */}
            </header>
            <main className="layout-content">
                {children}
            </main>
            <footer className="layout-footer">
                <p>&copy; {new Date().getFullYear()} My Application. All rights reserved.</p>
            </footer>
        </div>
    );
};

export default Layout;