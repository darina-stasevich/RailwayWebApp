import React from 'react';
import { NavLink } from 'react-router-dom';
import styles from './Navbar.module.css';

const Navbar = () => {

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('role');
        localStorage.removeItem('userName');
        console.log('Пользователь вышел, токен удален.');
    };
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
                <li className={styles.navItem}>
                    <NavLink
                        to="/login"
                        className={({ isActive}) =>  isActive ? `${styles.navLink} ${styles.active}` : styles.navLink}
                        onClick={handleLogout}
                    >
                        Выход
                    </NavLink>
                </li>
            </ul>
        </nav>
    );
};

export default Navbar;