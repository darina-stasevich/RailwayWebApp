import React from 'react';
import { useProfileData } from './hooks/useProfileData';
import ProfileCard from './components/ProfileCard';
import MessageDisplay from './components/MessageDisplay';
import styles from './ProfilePage.module.css';

const ProfilePage = () => {
    const {
        profileData,
        initialProfileData,
        isLoading,
        isSaving,
        isDeleting,
        error,
        message,
        genderOptions,
        handleChange,
        handleSubmit,
        handleDeleteAccount,
    } = useProfileData();

    if (isLoading && !initialProfileData) {
        return <div className={styles.loading}>Загрузка данных профиля...</div>;
    }

    if (!initialProfileData && !isLoading && !error && (!message || message.type !== 'error')) {
        return (
            <div className={styles.profileContainer}>
                <h1>Мой профиль</h1>
                <div className={styles.error}>Не удалось загрузить данные профиля. Попробуйте обновить страницу.</div>
            </div>
        );
    }

    return (
        <div className={styles.profileContainer}>
            <h1>Мой профиль</h1>

            <MessageDisplay message={message} error={error} />

            {initialProfileData && (
                <ProfileCard
                    profileData={profileData}
                    genderOptions={genderOptions}
                    isSaving={isSaving}
                    isLoading={isLoading}
                    isDeleting={isDeleting}
                    handleChange={handleChange}
                    handleSubmit={handleSubmit}
                    handleDelete={handleDeleteAccount}
                />
            )}
        </div>
    );
};

export default ProfilePage;