window.loginUser = async function(loginData) {
    try {
        const response = await fetch('/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include', // CRUCIAL: inclui cookies na requisição e resposta
            body: JSON.stringify({
                email: loginData.email,
                password: loginData.password,
                rememberMe: loginData.rememberMe
            })
        });

        if (response.ok) {
            // Cookie foi criado e enviado pelo servidor
            return 'success';
        } else {
            // Ler mensagem de erro do servidor
            const errorText = await response.text();
            return errorText || 'Invalid email or password. Please try again.';
        }
    } catch (error) {
        return `Unable to sign in right now. ${error.message}`;
    }
};

