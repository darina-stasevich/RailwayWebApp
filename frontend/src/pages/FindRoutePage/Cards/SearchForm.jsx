import styles from "./SearchForm.module.css";
import React from "react";
import {getFutureDateString} from "../../../utils/formatters.js";

const SearchForm = ({stations, fromStationId, onFromStationChange,
                     toStationId, onToStationChange,
                     departureDate, onDepartureDateChange,
                     isDirectRoute, onIsDirectRouteChange,
                     onSubmit,
                     isLoading}) => {
    return (
        <form onSubmit={onSubmit} className={styles.searchForm}>
            <div className={styles.formRow}>
                <div className={styles.formGroup}>
                    <label htmlFor="fromStation">Станция отправления:</label>
                    <select id="fromStation"
                            value={fromStationId}
                            onChange={(e) => onFromStationChange(e.target.value)}
                            required>
                        <option value="">Выберите станцию</option>
                        {stations.map((station) => (
                            <option key={station.id} value={station.id}>
                                {station.name} ({station.region})
                            </option>
                        ))}
                    </select>
                </div>
                <div className={styles.formGroup}>
                    <label htmlFor="toStation">Станция прибытия:</label>
                    <select
                        id="toStation"
                        value={toStationId}
                        onChange={(e) => onToStationChange(e.target.value)}
                        required>
                        <option value="">Выберите станцию</option>
                        {stations.map((station) => (
                            <option key={station.id} value={station.id}>
                                {station.name} ({station.region})
                            </option>
                        ))}
                    </select>
                </div>
            </div>
            <div className={styles.formRow}>
                <div className={styles.formGroup}>
                    <label htmlFor="departureDate">Дата отправления:</label>
                    <input
                        type="date"
                        id="departureDate"
                        value={departureDate}
                        onChange={(e) => onDepartureDateChange(e.target.value)}
                        required
                        min={new Date().toISOString().split('T')[0]}
                        max={getFutureDateString(30)}
                    />
                </div>
                <div className={`${styles.formGroup} ${styles.checkboxGroup}`}>
                    <input
                        type="checkbox"
                        id="isDirectRoute"
                        checked={isDirectRoute}
                        onChange={(e) => onIsDirectRouteChange(e.target.checked)}
                    />
                    <label htmlFor="isDirectRoute">Только прямой маршрут</label>
                </div>
            </div>

            <button type="submit" disabled={isLoading} className={styles.searchButton}>
                {isLoading ? 'Поиск...' : 'Найти маршруты'}
            </button>
        </form>
    )
};
export default SearchForm;
