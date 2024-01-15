import { notification } from 'antd';

export const notify = {
    success,
    error
};

function success(m) {
    notification.success({
        message: "Sucess",
        description: m
    });    
}

function error(m) {
    console.log(m);
    notification.error({
        message: "Error",
        description: m
    });
}