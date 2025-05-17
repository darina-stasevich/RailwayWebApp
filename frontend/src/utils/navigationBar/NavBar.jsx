import React from 'react';
import { NavLink } from 'react-router-dom';
import styles from './Navbar.module.css';

const Navbar = () => {
    return (
        <nav className={styles.navbar}>
            <ul className={styles.navList}>
                <li className={styles.navItem}>
                    <NavLink
                        to="/find-route"
                        className={({ isActive }) => isActive ? `${styles.navLink} ${styles.active}` : styles.navLink}
                    >
                        Поиск маршрута
                    </NavLink>
                </li>
                <li className={styles.navItem}>
                    <NavLink
                        to="/my-bookings"
                        className={({ isActive }) => isActive ? `${styles.navLink} ${styles.active}` : styles.navLink}
                    >
                        Бронирования
                    </NavLink>
                </li>
                <li className={styles.navItem}>
                    <NavLink
                        to="/my-tickets"
                        className={({ isActive }) => isActive ? `${styles.navLink} ${styles.active}` : styles.navLink}
                    >
                        Билеты
                    </NavLink>
                </li>
                <li className={styles.navItem}>
                    <NavLink
                        to="/my-profile"
                        className={({ isActive }) => isActive ? `${styles.navLink} ${styles.active}` : styles.navLink}
                    >
                        Профиль
                    </NavLink>
                </li>
                {/* Можно добавить ссылку для выхода, если потребуется */}
                {/* <li className={styles.navItem} style={{ marginLeft: 'auto' }}>
                    <NavLink to="/logout" className={styles.navLink}>Выход</NavLink>
                </li> */}
            </ul>
        </nav>
    );
};

export default Navbar;