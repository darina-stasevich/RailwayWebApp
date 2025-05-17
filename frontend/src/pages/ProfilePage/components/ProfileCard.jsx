import React from 'react';
import styles from './ProfileCard.module.css';
import buttonStyles from '../../../styles/ButtonStyles.module.css';

const ProfileCard = ({
                         profileData,
                         genderOptions,
                         isSaving,
                         isDeleting,
                         isLoading,
                         handleChange,
                         handleSubmit,
                         handleDelete,
                         openPasswordModal
                     }) => {
    return (
        <form onSubmit={handleSubmit} className={styles.profileForm}>
            <div className={styles.formGroup}>
                <label htmlFor="email">Email:</label>
                <input type="email" id="email" name="email" value={profileData.email || ''} onChange={handleChange} readOnly />
            </div>
            <div className={styles.formGroup}>
                <label htmlFor="surname">Фамилия:</label>
                <input type="text" id="surname" name="surname" value={profileData.surname || ''} onChange={handleChange} />
            </div>
            <div className={styles.formGroup}>
                <label htmlFor="name">Имя:</label>
                <input type="text" id="name" name="name" value={profileData.name || ''} onChange={handleChange} />
            </div>
            <div className={styles.formGroup}>
                <label htmlFor="secondName">Отчество (если есть):</label>
                <input type="text" id="secondName" name="secondName" value={profileData.secondName || ''} onChange={handleChange} />
            </div>
            <div className={styles.formGroup}>
                <label htmlFor="phoneNumber">Номер телефона:</label>
                <input type="tel" id="phoneNumber" name="phoneNumber" value={profileData.phoneNumber || ''} onChange={handleChange} placeholder="+375 XX XXXXXXX" />
            </div>
            <div className={styles.formGroup}>
                <label htmlFor="birthDate">Дата рождения:</label>
                <input type="date" id="birthDate" name="birthDate" value={profileData.birthDate} onChange={handleChange} />
            </div>
            <div className={styles.formGroup}>
                <label htmlFor="gender">Пол:</label>
                <select id="gender" name="gender" value={profileData.gender} onChange={handleChange}>
                    {genderOptions.map(opt => (
                        <option key={opt.value} value={opt.value}>{opt.label}</option>
                    ))}
                </select>
            </div>


            <div className={styles.formActions}>
                <button
                    type="submit"
                    className={buttonStyles.confirmButton}
                    disabled={isSaving || isLoading || isDeleting}
                >
                    {isSaving ? 'Сохранение...' : (isLoading ? 'Загрузка...' : 'Сохранить изменения')}
                </button>
                <button
                    type="button"
                    onClick={openPasswordModal}
                    className={buttonStyles.secondaryButton}
                    disabled={isSaving || isLoading || isDeleting}
                >
                    Изменить пароль
                </button>
                <button
                    type="button"
                    onClick={handleDelete}
                    className={buttonStyles.cancelButton}
                    disabled={isDeleting || isSaving || isLoading}
                >
                    {isDeleting ? 'Удаление...' : 'Удалить аккаунт'}
                </button>

            </div>
        </form>
    );
};

export default ProfileCard;