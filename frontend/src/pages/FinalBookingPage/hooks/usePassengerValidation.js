import { useState, useEffect } from 'react';

export const usePassengerValidation = (passengerData) => {
    const [errors, setErrors] = useState({});
    const [isValid, setIsValid] = useState(false);

    useEffect(() => {
        const validationErrors = {};

        if (!passengerData.surname) {
            validationErrors.surname = 'Фамилия обязательна';
        } else if (passengerData.surname.length < 2) {
            validationErrors.surname = 'Минимальная длина фамилии - 2 символа';
        } else if (passengerData.surname.length > 40) {
            validationErrors.surname = 'Максимальная длина фамилии - 40 символов';
        }

        if (!passengerData.name) {
            validationErrors.name = 'Имя обязательно';
        } else if (passengerData.name.length < 2) {
            validationErrors.name = 'Минимальная длина имени - 2 символа';
        } else if (passengerData.name.length > 40) {
            validationErrors.name = 'Максимальная длина имени - 40 символов';
        }

        if (passengerData.secondName) {
            if (passengerData.secondName.length < 2) {
                validationErrors.secondName = 'Минимальная длина отчества - 2 символа';
            } else if (passengerData.secondName.length > 40) {
                validationErrors.secondName = 'Максимальная длина отчества - 40 символов';
            }
        }

        if (passengerData.gender === '') {
            validationErrors.gender = 'Выберите пол';
        }

        if (!passengerData.birthDate) {
            validationErrors.birthDate = 'Укажите дату рождения';
        } else {
            const birthDate = new Date(passengerData.birthDate);
            const today = new Date();
            const age = today.getFullYear() - birthDate.getFullYear();

            if (isNaN(birthDate.getTime())) {
                validationErrors.birthDate = 'Некорректная дата';
            } else if (age > 120) {
                validationErrors.birthDate = 'Возраст не может превышать 120 лет';
            } else if (age < 0) {
                validationErrors.birthDate = 'Дата рождения не может быть в будущем';
            }
        }

        const passportRegex = /^[A-Z]{2}\d{7}$/;
        if (!passengerData.passportNumber) {
            validationErrors.passportNumber = 'Укажите номер паспорта';
        } else if (!passportRegex.test(passengerData.passportNumber)) {
            validationErrors.passportNumber = 'Неверный формат. Должно быть 2 заглавные буквы и 7 цифр (например, AB1234567)';
        }

        setErrors(validationErrors);
        setIsValid(Object.keys(validationErrors).length === 0);

    }, [passengerData]);

    return { errors, isValid };
};