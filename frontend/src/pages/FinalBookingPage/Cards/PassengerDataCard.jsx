import React from 'react';
import {formatDateTime, getFutureDateString} from '../../../utils/formatters.js';
import { usePassengerValidation } from '../hooks/usePassengerValidation.js';
import styles from './PassengerDataCard.module.css';

const PassengerDataCard = ({
                               form,
                               index,
                               handleInputChange,
                               formId
                           }) => {
    const { errors, isValid } = usePassengerValidation(form.passengerData);

    return (
        <div
            className={styles.passengerFormCard}
            data-form-valid={isValid}
            data-form-id={formId}
        >
            <div className={styles.ticketHeader}>
                <h3>Билет <span className={styles.ticketNumber}>{index + 1}</span></h3>
            </div>

            <div className={styles.ticketSegment}>
                <div className={styles.routeInfo}>
                    <p>
                        <span className={styles.routeLabel}>Маршрут:</span>
                        <strong>{form.displayInfo.fromStationName} → {form.displayInfo.toStationName}</strong>
                    </p>
                    <p>
                        <span className={styles.routeLabel}>Отправление:</span>
                        {formatDateTime(form.displayInfo.departureDate)}
                    </p>
                    <p>
                        <span className={styles.routeLabel}>Прибытие:</span>
                        {formatDateTime(form.displayInfo.arrivalDate)}
                    </p>
                    <p>
                        <span className={styles.routeLabel}>Вагон:</span>
                        <strong>{form.carriageNumber}</strong>,
                        <span className={styles.routeLabel}>Место:</span>
                        <strong>{form.seatNumber}</strong>
                    </p>
                </div>

                <div className={styles.passengerInfo}>
                    <div className={styles.formRow}>
                        <label className={errors.surname ? styles.errorField : ''}>
                            Фамилия:
                            <input
                                type="text"
                                value={form.passengerData.surname}
                                onChange={(e) => handleInputChange(form.formId, 'passengerData.surname', e.target.value)}
                                required
                            />
                            {errors.surname && <span className={styles.errorText}>{errors.surname}</span>}
                        </label>

                        <label className={errors.firstName ? styles.errorField : ''}>
                            Имя:
                            <input
                                type="text"
                                value={form.passengerData.firstName}
                                onChange={(e) => handleInputChange(form.formId, 'passengerData.firstName', e.target.value)}
                                required
                            />
                            {errors.firstName && <span className={styles.errorText}>{errors.firstName}</span>}
                        </label>

                        <label className={errors.secondName ? styles.errorField : ''}>
                            Отчество (если есть):
                            <input
                                type="text"
                                value={form.passengerData.secondName || ''}
                                onChange={(e) => handleInputChange(form.formId, 'passengerData.secondName', e.target.value || null)}
                            />
                            {errors.secondName && <span className={styles.errorText}>{errors.secondName}</span>}
                        </label>
                    </div>

                    <div className={styles.formRow}>
                        <label className={errors.gender ? styles.errorField : ''}>
                            Пол:
                            <select
                                value={String(form.passengerData.gender)}
                                onChange={(e) => handleInputChange(form.formId, 'passengerData.gender', e.target.value)}
                                required
                            >
                                <option value="">Выберите пол</option>
                                <option value="0">Мужской</option>
                                <option value="1">Женский</option>
                            </select>
                            {errors.gender && <span className={styles.errorText}>{errors.gender}</span>}
                        </label>

                        <label className={errors.birthDate ? styles.errorField : ''}>
                            Дата рождения:
                            <input
                                type="date"
                                value={form.passengerData.birthDate}
                                onChange={(e) => handleInputChange(form.formId, 'passengerData.birthDate', e.target.value)}
                                required
                                max={getFutureDateString(0)}
                            />
                            {errors.birthDate && <span className={styles.errorText}>{errors.birthDate}</span>}
                        </label>
                    </div>

                    <div className={styles.formRow}>
                        <label className={`${styles.fullWidth} ${errors.passportNumber ? styles.errorField : ''}`}>
                            Номер паспорта (серия и номер):
                            <input
                                type="text"
                                value={form.passengerData.passportNumber}
                                onChange={(e) => handleInputChange(form.formId, 'passengerData.passportNumber', e.target.value)}
                                required
                                placeholder="AB1234567"
                            />
                            {errors.passportNumber && <span className={styles.errorText}>{errors.passportNumber}</span>}
                        </label>
                    </div>

                    <label className={styles.bedLinenLabel}>
                        <input
                            type="checkbox"
                            checked={form.hasBedLinenSet}
                            onChange={(e) => handleInputChange(form.formId, 'hasBedLinenSet', e.target.checked)}
                        />
                        Включить постельное белье
                    </label>
                </div>
            </div>

            {!isValid && (
                <div className={styles.cardValidationStatus}>
                    Заполните все обязательные поля
                </div>
            )}
        </div>
    );
};

export default PassengerDataCard;